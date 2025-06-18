<#
.SYNOPSIS
    Deploy Network Perspective Worker container instance with Azure KeyVault.

.DESCRIPTION
    This script automates the deployment of Network Perspective Worker infrastructure including:
    - Azure Resource Group creation
    - KeyVault with RBAC authorization
    - EntraId Application registration with required API permissions
    - Container Instance with managed identity and KeyVault integration

.PARAMETER ConfigurationFile
    Path to the YAML configuration file for the deployment

.NOTES
    - Prerequisites: Azure CLI 2.0 (https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
    - YAML module: powershell-yaml
    - Usage:
        1. Login to Azure CLI: az login
        2. Configure the configuration.yaml file
        3. Run the script: ./deploy-worker.ps1 -configurationFile "./configuration.yaml"
        4. Grant admin consent to the created application

.EXAMPLE
    ./deploy-worker.ps1 -configurationFile "./configuration.yaml"
#>
Param(    
    [Parameter()]
    [string] $configurationFile = "./configuration.yaml"
)

# Install and import the powershell-yaml module if not already available
if (-not (Get-Module -ListAvailable -Name powershell-yaml)) {
    Write-Host "Installing PowerShell-Yaml module..."
    Install-Module -Name powershell-yaml -Force -Scope CurrentUser
}

Import-Module powershell-yaml

# Validate and read the YAML configuration file
if (-not (Test-Path $configurationFile)) {
    Write-Error "Configuration file not found: $configurationFile"
    exit 1
}

try {
    # Read and parse the YAML file
    $config = (Get-Content -Raw -Path $configurationFile) | ConvertFrom-Yaml
    
    # Extract key configuration parameters
    $resourceGroupName = $config.configuration.resourceGroupName
    $location = $config.configuration.region
    $teamsPermissions = $config.configuration.teamsPermissions    
    
    # Validate mandatory configuration values
    if ([string]::IsNullOrEmpty($resourceGroupName)) {
        Write-Error "Resource group name not specified in configuration"
        exit 1
    }
    
    if ([string]::IsNullOrEmpty($location)) {
        Write-Host "Location not specified in configuration. "
        exit 1
    }
} catch {
    Write-Error "Failed to parse configuration file: $_"
    exit 1
}


# Function to generate a unique random looking, yet deterministic string
function Get-UniqueString ([string]$id, $length=8)
{
    $hashArray = (new-object System.Security.Cryptography.SHA512Managed).ComputeHash($id.ToCharArray())
        -join ($hashArray[1..$length] | ForEach-Object { [char]($_ % 26 + [byte][char]'a') })
}

# returns keyvault name
function Setup-KeyVault
{        
    Param(
        [Parameter()]
        [string] $keyVaultName,

        [Parameter()]
        $config
    )

    # check if keyvault exists
    $publicIp = $null
    $keyVault = az keyvault list --resource-group $config.configuration.resourceGroupName --query "[?name=='$keyVaultName']" | ConvertFrom-Json
    if ($null -ne $keyVault) {
        Write-Host "✅ KeyVault $keyVaultName already exists"    
    } else {    
        $keyVault = az keyvault create --name $keyVaultName --resource-group $config.configuration.resourceGroupName --location $config.configuration.region | ConvertFrom-Json
        Write-Host "✅ KeyVault created with RBAC role assigned to current user"
    }

    # Get current user's Object ID
    Write-Host "Assigning Key Vault Secrets Officer role to current user"
    $currentUserObjectId = az ad signed-in-user show --query id -o tsv
    # Assign Key Vault Secrets Officer role to current user (allows get/set operations)
    az role assignment create --assignee $currentUserObjectId --role "Key Vault Secrets Officer" --scope $keyVault.id

    # Update KeyVault to allow access from current ip
    $publicIp = (Invoke-WebRequest ifconfig.me/ip).Content.Trim()
    Write-Host "Updating KeyVault firewall to allow access from current ip $publicIp"
    az keyvault network-rule add --name $keyVaultName --ip-address $publicIp | Out-Null
    # az keyvault update --name $keyVaultName --resource-group $config.configuration.resourceGroupName --default-action Allow | Out-Null

    # Create the KeyVault secrets
    Write-Host "Creating KeyVault secrets..."

    Write-Host "- health check"
    az keyvault secret set --vault-name $keyVault.name --name "health-check" --value "OK" | Out-Null

    Write-Host "- hashing key"
    $hashingKeySecret = az keyvault secret show --vault-name $keyVault.name --name "hashing-key" 2> $null | ConvertFrom-Json
    if ($null -eq $hashingKeySecret) {    
        $hashingKey = [guid]::NewGuid().ToString() # create a hashing key as a random guid
        az keyvault secret set --vault-name $keyVault.name --name "hashing-key" --value $hashingKey | Out-Null
    }

    Write-Host "- orchestrator client name"
    $orchestratorClientName = $config.keyvault["orchestrator-client-name"]
    az keyvault secret set --vault-name $keyVault.name --name "orchestrator-client-name" --value $orchestratorClientName | Out-Null

    Write-Host "- orchestrator client secret"
    $orchestratorClientSecret = $config.keyvault["orchestrator-client-secret"]
    az keyvault secret set --vault-name $keyVault.name --name "orchestrator-client-secret" --value $orchestratorClientSecret | Out-Null

    Write-Host "✅ Secrets saved in KeyVault"
    
    return @{
        KeyVaultName = $keyVault.name
        PublicIp = $publicIp
    }
}

function Register-Application 
{
    Param(
        [string]$appName,
        [string]$appCallbackUri,
        [string]$keyVaultName,
        [bool]$teamsPermissions     
    )
    
    Write-Host "Registering application '$appName'"

    $secretPrefix = $teamsPermissions ? "microsoft-client-with-teams" : "microsoft-client-basic"

    # Check if app already exists
    $appId = az ad app list --display-name $appName --query "[].appId"  --output tsv

    if ($null -ne $appId) {
        # ask for confirmation to delete the app
        Write-Host "Application '$appName' already exists"
        Write-Host
        $confirmation = Read-Host "🚨 Do you want to delete and recreate the existing app? (y/n)"
        if ($confirmation -eq "y") {
            Write-Host "Deleting app with id $appId"
            az ad app delete --id $appId
            Write-Host "✅ Prev app deleted"
        }
        else {
            Write-Host "✅ Using existing app"
        }
    }  
    
    # 1. Create Azure AD Application
    Write-Host "Creating Entra id application"         
    $app = az ad app create --display-name $appName `
        --sign-in-audience "AzureADMyOrg" `
        --web-redirect-uris $appCallbackUri `
        --enable-id-token-issuance true `
        | ConvertFrom-Json

    $appId = $app.appId    

    Write-Host "Adding permissions to the app"

    # check existing app permissions and create missing permissions
    $existingPermissions = az ad app permission list --id $appId --query "[].resourceAccess[].id" --output tsv

    function Add-Permission($permissionId, $permissionName) {
        if ($existingPermissions -contains $permissionId) {
            Write-Host "- $permissionName already exists"
        } else {
            Write-Host "- $permissionName"
            $msGraphApi = "00000003-0000-0000-c000-000000000000"
            az ad app permission add --id $appId --api $msGraphApi --api-permissions "$permissionId=Role" 2> $null
        }
    }

    Add-Permission "df021288-bdef-4463-88db-98f22de89214" "User.Read.All"
    Add-Permission "8ba4a692-bc31-4128-9094-475872af8a53" "Calendars.ReadBasic.All"
    Add-Permission "693c5e45-0940-467d-9b8a-1022fb9d42ef" "Mail.ReadBasic.All"
    Add-Permission "98830695-27a2-44f7-8c18-0c3ebc9698f6" "GroupMember.Read.All"

    if ($teamsPermissions -eq $true) {
        Add-Permission "2280dda6-0bfd-44ee-a2f4-cb867cfc4c1e" "Team.ReadBasic.All"
        Add-Permission "59a6b24b-4225-4393-8165-ebaec5f55d7a" "Channel.ReadBasic.All"
        Add-Permission "3b55498e-47ec-484f-8136-9013221c06a9" "ChannelMember.Read.All"
        Add-Permission "7b2449af-6ccd-4f4d-9f78-e550c193f0d1" "ChannelMessage.Read.All"
        Add-Permission "6b7d71aa-70aa-4810-a8d9-5d9fb2830017" "Chat.Read.All"
    }

    Write-Host "✅ App permissions added"

    Write-Host "Saving secrets in KeyVault"
    Write-Host "- app id"
    az keyvault secret set --vault-name $keyVaultName --name "$($secretPrefix)-id" --value $appId | Out-Null
    Write-Host "- app secret"
    $appSecret = az ad app credential reset --id $appId --display-name "app-secret" 2> $null | ConvertFrom-Json 
    az keyvault secret set --vault-name $keyVaultName --name "$($secretPrefix)-secret" --value $appSecret.password | Out-Null

    Write-Host "✅ Application configured"
    Write-Host
    return $appId
}

Write-Host "Creating Azure resources ---------------------------------------------"
Write-Host

# Create the resource group if it doesn't exist
Write-Host "Creating resource group $resourceGroupName in $location"
try {
    # Check if resource group exists
    $existingGroup = az group show --name $resourceGroupName 2>$null | ConvertFrom-Json
    if ($null -eq $existingGroup) {
        # Create resource group
        az group create --name $resourceGroupName --location $location | Out-Null
        Write-Host "✅ Resource group created"
    } else {
        Write-Host "✅ Resource group already exists"
    }
    
    # Get resource group ID regardless of whether it was just created or already existed
    $resourceGroupId = az group show --name $resourceGroupName --query id --output tsv
    if ([string]::IsNullOrEmpty($resourceGroupId)) {
        throw "Failed to get resource group ID"
    }
} catch {
    Write-Error "Error creating/accessing resource group: $_"
    exit 1
}

# Generate a unique ID for KeyVault naming
$uniqueId = $(Get-UniqueString($resourceGroupId))
$keyVaultName = "kv-np-$uniqueId"
Write-Host "Using KeyVault name: $keyVaultName"

# Setup KeyVault
try {
    $keyVaultResult = Setup-KeyVault -keyVaultName $keyVaultName -config $config
    $keyVaultName = $keyVaultResult.KeyVaultName
    $publicIp = $keyVaultResult.PublicIp
    
    if ([string]::IsNullOrEmpty($keyVaultName)) {
        throw "KeyVault name is empty or null"
    }
    
    Write-Host "✅ KeyVault setup completed"
    Write-Host
} catch {
    Write-Error "Error setting up KeyVault: $_"
    exit 1
}

Write-Host "Registering EntraId application -------------------------------------"
Write-Host
try {
    Register-Application -appName $config.configuration.entraAppName -appCallbackUri $config.configuration.entraAppCallbackUri -keyVaultName $keyVaultName -teamsPermissions $teamsPermissions
    Write-Host "✅ Application registration completed"
} catch {
    Write-Error "Error registering application: $_"
    Write-Warning "Continuing with deployment despite application registration issues"
}

# Prepare parameters for bicep deployment
Write-Host
Write-Host "Deploying infrastructure ----------------------------------------"
Write-Host

# Get uniqueId from resource group name if not already set
if ([string]::IsNullOrEmpty($uniqueId)) {
    $uniqueId = $resourceGroupName -replace '.*-' # Extract identifier from resource group name
    $uniqueId = $uniqueId.ToLower()
}

$bicepFilePath = "./np-worker.bicep"
if (-not (Test-Path $bicepFilePath)) {
    Write-Error "Bicep template not found: $bicepFilePath"
    exit 1
}

# Format parameters for Azure CLI using parameter file
try {
    # Create a temporary parameters file with proper bicep schema
    $tempParamFile = [System.IO.Path]::GetTempFileName() + ".json"
    
    # Create parameters in proper bicep format
    $bicepParams = @{
        '$schema' = "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#"
        'contentVersion' = "1.0.0.0"
        'parameters' = @{}
    }
    
    # Add each parameter with the required value structure
    $bicepParams.parameters.keyVaultName = @{ value = $keyVaultName }
    $bicepParams.parameters.uniqueId = @{ value = $uniqueId }
    $bicepParams.parameters.containerImageTag = @{ value = $config.configuration.containerImageTag }
    $bicepParams.parameters.registryUsername = @{ value = $config.configuration.registryUsername }
    $bicepParams.parameters.registryPassword = @{ value = $config.configuration.registryPassword }
    $bicepParams.parameters.networkPerspectiveApiUrl = @{ value = $config.configuration.networkPerspectiveApiUrl }
    $bicepParams.parameters.applicationInsightsConnectionString = @{ value = $config.configuration.applicationInsightsConnectionString }
    $bicepParams.parameters.applicationInsightsRoleInstance = @{ value = $config.configuration.applicationInsightsRoleInstance }
    
    # Save to file
    $bicepParams | ConvertTo-Json -Depth 10 | Out-File $tempParamFile -Encoding utf8
    
    Write-Host "Created parameter file: $tempParamFile"
    
    # Execute deployment
    Write-Host "Running deployment..."
    
    Write-Host "Running command: "
    Write-Host "az deployment group create --resource-group $resourceGroupName --template-file $bicepFilePath --parameters @$tempParamFile"
    
    az deployment group create `
        --resource-group $resourceGroupName `
        --template-file $bicepFilePath `
        --parameters "@$tempParamFile" `
        --output json | ConvertFrom-Json        
    
    # Clean up temporary file
    Remove-Item $tempParamFile -Force
    
    Write-Host "✅ Infrastructure deployed successfully"
    
    # Print deployment information
    Write-Host ""
    Write-Host "----------------------------------------------------------------------"
    Write-Host "✅ DEPLOYMENT COMPLETED SUCCESSFULLY"
    Write-Host "- Resource Group: $resourceGroupName"
    Write-Host 
    Write-Host "To check container logs run:"
    Write-Host "az container logs --resource-group $resourceGroupName --name np-worker-$uniqueId-cg"
    Write-Host 
    Write-Host "Recent logs... (in 10 seconds...)"

    # Wait for container to initialize
    Start-Sleep -Seconds 10
    az container logs --resource-group $resourceGroupName --name np-worker-$uniqueId-cg            
    
} catch {
    Write-Error "Deployment failed: $_"
    exit 1
}

Write-Host "----------------------------------------------------------------------"
Write-Host
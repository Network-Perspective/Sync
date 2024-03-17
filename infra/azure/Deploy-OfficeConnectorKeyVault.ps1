<#
.SYNOPSIS
    This script creates an Azure EntraId Application and stores the client id and secret in a KeyVault.

.DESCRIPTION
    This script automates the process of creating an Azure AD Application and configuring a KeyVault to store the client id and secret. It also registers the application with the necessary permissions.

.PARAMETER networkId
    The id of the network id (company id) to be synced.

.PARAMETER resourceGroupName
    The name of the resource group where the KeyVault will be created. Default value is "rg-networkperspective".

.PARAMETER location
    The location where the resource group and KeyVault will be created e.g. "westeurope".

.PARAMETER firewallIp
    The IP address to be added to the KeyVault firewall rules. 

.NOTES
    - Prerequisites: Azure CLI 2.0 (https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
    - Usage:
        1. Login to Azure CLI
        2. Run the script
        3. Open the URL in the output to grant admin consent

.EXAMPLE
    ./Deploy-OfficeConnectorKeyVault.ps1 -networkId "28b7d621-46d2-4fd4-8815-3148eefa8d6c" -resourceGroupName "myResourceGroup" -location "westeurope"
#>
Param(
    [Parameter(Mandatory=$true)]
    [string] $networkId, 

    [Parameter()]
    [string] $resourceGroupName = "rg-networkperspective",

    [Parameter()]
    [string] $location = "germanywestcentral",

    [Parameter()]
    [string] $firewallIp = "20.79.225.18/32",

    [Parameter()]
    [string] $appCallbackUri = "https://app.networkperspective.io/sync/Office365Sync/Callback",

    [Parameter()]
    [string] $npServicePrincipalName = "Network Perspective Connectors",

    [Parameter()]
    [string] $npServicePrincipalId = "018cbfba-1da5-4eef-b6c7-00bbd2cfa661"    
)

# Constants
$appNameBasic = "Network Perspective Office 365 Sync"
$appNameWithTeams = "Network Perspective Office 365 Teams Sync"

# Function to generate a unique random looking, yet deterministic string
function Get-UniqueString ([string]$id, $length=13)
{
    $hashArray = (new-object System.Security.Cryptography.SHA512Managed).ComputeHash($id.ToCharArray())
        -join ($hashArray[1..$length] | ForEach-Object { [char]($_ % 26 + [byte][char]'a') })
}

# returns keyvault name
function Create-AzureResources
{        
    # Create the resource group
    Write-Host "Creating resource group $resourceGroupName in $location"
    az group create --name $resourceGroupName --location $location | Out-Null
    $resourceGroupId = az group show --name $resourceGroupName --query id --output tsv 
    Write-Host "✅ Resource group created"

    # Create the KeyVault
    $keyVaultName = "kv-np-$(Get-UniqueString($resourceGroupId))"
    Write-Host "Creating KeyVault with name $keyVaultName"

    # check if keyvault exists
    $publicIp = $null
    $keyVault = az keyvault list --resource-group $resourceGroupName --query "[?name=='$keyVaultName']" | ConvertFrom-Json
    if ($null -ne $keyVault) {
        Write-Host "✅ KeyVault $keyVaultName already exists"    
        $publicIp = (Invoke-WebRequest ifconfig.me/ip).Content.Trim()
        Write-Host "Updating KeyVault firewall to allow access from current ip $publicIp"
        az keyvault network-rule add --name $keyVaultName --ip-address $publicIp | Out-Null
        az keyvault update --name $keyVaultName --resource-group $resourceGroupName --default-action Allow | Out-Null
    } else {    
        $keyVault = az keyvault create --name $keyVaultName --resource-group $resourceGroupName --location $location | ConvertFrom-Json
        Write-Host "✅ KeyVault created"
    }

    # Create the KeyVault secrets
    Write-Host "- health check"
    az keyvault secret set --vault-name $keyVault.name --name "health-check" --value "OK" | Out-Null
    Write-Host "- hashing key"
    $hashingKeySecret = az keyvault secret show --vault-name $keyVault.name --name "hashing-key" 2> $null | ConvertFrom-Json
    if ($null -eq $hashingKeySecret) {    
        $hashingKey = [guid]::NewGuid().ToString() # create a hashing key as a random guid
        az keyvault secret set --vault-name $keyVault.name --name "hashing-key" --value $hashingKey | Out-Null
    }
    Write-Host "- tenant id"    
    $tenantId = az account show --query tenantId -o tsv
    $tenantKey = "microsoft-tenant-id-$networkId"
    az keyvault secret set --vault-name $keyVault.name --name $tenantKey --value $tenantId | Out-Null
    Write-Host "✅ Secrets saved in KeyVault"

    # Create the service principal
    Write-Host "Creating service principal"
    # check if service principal exists
    $servicePrincipals = az ad sp list --display-name $npServicePrincipalName --query "[].appId" --output tsv
    if ($null -ne $servicePrincipals) {
        Write-Host "✅ Service principal already exists"
    } else {
        az ad sp create --id $npServicePrincipalId | Out-Null
        Write-Host "✅ Service principal created"
    }

    # Grant permissions to the service principal
    Write-Host "Granting permissions to access the KeyVault"
    az keyvault set-policy --name $keyVault.name --spn $npServicePrincipalId --secret-permissions get list set | Out-Null
    Write-Host "✅ Permissions granted"

    return @{
        KeyVaultName = $keyVault.name
        PublicIp = $publicIp
    }
}

function Register-Appliation 
{
    Param(
        [string]$appName,
        [string]$keyVaultName,
        [bool]$teamsPermissions,
        [string]$secretPrefix 
    )
    
    Write-Host "Registering application '$appName'"

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

    Write-Host "Addding permissions to the app"

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

Write-Host "Script confifuration -------------------------------------------------"
Write-Host "- Network id: $networkId"
Write-Host "- Resource group: $resourceGroupName"
Write-Host "- Location: $location"
Write-Host "- Firewall IP: $firewallIp"
Write-Host "- App callback URI: $appCallbackUri"
Write-Host "- Service principal name: $npServicePrincipalName"
Write-Host "- Service principal id: $npServicePrincipalId"
Write-Host

Write-Host "Creating azure resources ---------------------------------------------"
Write-Host
$azureResources = Create-AzureResources
$keyVaultName = $azureResources.KeyVaultName
$publicIp = $azureResources.PublicIp
Write-Host "✅ Azure resources created"
Write-Host

Write-Host "Registering EntraId applications -------------------------------------"
Write-Host
$appIdBasic     = Register-Appliation -appName $appNameBasic -keyVaultName $keyVaultName -teamsPermissions $false -secretPrefix "microsoft-client-basic"
$appIdWithTeams = Register-Appliation -appName $appNameWithTeams -keyVaultName $keyVaultName -teamsPermissions $true -secretPrefix "microsoft-client-with-teams"

# Finally configure KeyVault firewall
Write-Host "Configuring KeyVault firewall ----------------------------------------"
az keyvault network-rule add --name $keyVaultName --ip-address $firewallIp | Out-Null
az keyvault update --name $keyVaultName --resource-group $resourceGroupName --default-action Deny | Out-Null

Write-Host "Public ip: $publicIp"
if ($null -ne $publicIp) {
    Write-Host "Removing firewall rule that allows access from current ip $publicIp"
    az keyvault network-rule remove --name $keyVaultName --ip-address $publicIp | Out-Null
}

Write-Host "✅ KeyVault firewall configured"

# Print info about manual steps
Write-Host 
Write-Host "----------------------------------------------------------------------"
Write-Host "🚨🚨🚨  To grant admin consent for the app open the following URL in a browser  🚨🚨🚨"
Write-Host
Write-Host "- Mail & calendar metadata sync:"
Write-Host "https://login.microsoftonline.com/common/adminconsent?client_id=$appIdBasic"
Write-Host 
Write-Host "- Teams metadata sync:"
Write-Host "https://login.microsoftonline.com/common/adminconsent?client_id=$appIdWithTeams"
Write-Host
Write-Host "🚨🚨🚨  Use the keyvault name below to configure the connector"
Write-Host "- KeyVault name: $keyVaultName"
Write-Host
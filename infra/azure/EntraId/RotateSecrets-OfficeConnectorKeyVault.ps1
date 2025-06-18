<#
.SYNOPSIS
    This script rotates the Office Connector KeyVault secret.

.DESCRIPTION
    This script automates the process of rotating the Office Connector KeyVault secret.

.PARAMETER resourceGroupName
    The name of the resource group where the KeyVault is located. Default value is "rg-networkperspective".

.PARAMETER firewallIp
    The IP address to be added to the KeyVault firewall rules. 

.NOTES
    - Prerequisites: Azure CLI 2.0 (https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
    - Usage:
        1. Login to Azure CLI
        2. Run the script
        3. Open the URL in the output to grant admin consent

.EXAMPLE
    ./RotateSecrets-OfficeConnectorKeyVault.ps1 -resourceGroupName "myResourceGroup" 
#>
Param(
    [Parameter()]
    [string] $resourceGroupName = "rg-networkperspective",

    [Parameter()]
    [string] $firewallIp = "20.79.225.18/32"
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
function Locate-AzureResources
{        
    # Create the resource group    
    $resourceGroupId = az group show --name $resourceGroupName --query id --output tsv 

    if ($null -eq $resourceGroupId) {
        Write-Host "⛔ Resource group not found"
        exit 1
    }

    # Create the KeyVault
    $keyVaultName = "kv-np-$(Get-UniqueString($resourceGroupId))"
    Write-Host "Locating KeyVault with name $keyVaultName"

    # check if keyvault exists
    $publicIp = $null
    $keyVault = az keyvault list --resource-group $resourceGroupName --query "[?name=='$keyVaultName']" | ConvertFrom-Json
    if ($null -eq $keyVault) 
    { 
        Write-Host "⛔ KeyVault not found"
        exit 1
    }

    Write-Host "✅  KeyVault found"
    $publicIp = (Invoke-WebRequest ifconfig.me/ip).Content.Trim()
    Write-Host "Updating KeyVault firewall to allow access from current ip $publicIp"
    az keyvault network-rule add --name $keyVaultName --ip-address $publicIp | Out-Null
    az keyvault update --name $keyVaultName --resource-group $resourceGroupName --default-action Allow | Out-Null

    return @{
        KeyVaultName = $keyVault.name
        PublicIp = $publicIp
    }
}

function Rotate-SecretForApplication 
{
    Param(
        [string]$appName,
        [string]$keyVaultName,
        [bool]$teamsPermissions,
        [string]$secretPrefix 
    )
    
    Write-Host "Updating application '$appName'"

    # Check if app already exists
    $appId = az ad app list --display-name $appName --query "[].appId"  --output tsv

    if ($null -eq $appId) 
    {
        Write-Host "⛔ Application '$appName' not found"
        exit 1 
    }
    
    Write-Host "Resetting app secret for '$appName'"
    $appSecret = az ad app credential reset --id $appId --display-name "app-secret" 2> $null | ConvertFrom-Json 

    Write-Host "Saving secrets in KeyVault"
    Write-Host "- app secret"
    az keyvault secret set --vault-name $keyVaultName --name "$($secretPrefix)-secret" --value $appSecret.password --only-show-errors | Out-Null

    Write-Host "✅  Application secret rotated"
    Write-Host    
}

Write-Host "Script confifuration"
Write-Host "- Resource group: $resourceGroupName"
Write-Host "- Firewall IP: $firewallIp"
Write-Host

$azureResources = Locate-AzureResources
$keyVaultName = $azureResources.KeyVaultName
$publicIp = $azureResources.PublicIp

Write-Host
Write-Host "Rotating secrets for EntraId applications"
Write-Host

Rotate-SecretForApplication -appName $appNameBasic -keyVaultName $keyVaultName -teamsPermissions $false -secretPrefix "microsoft-client-basic"
Rotate-SecretForApplication -appName $appNameWithTeams -keyVaultName $keyVaultName -teamsPermissions $true -secretPrefix "microsoft-client-with-teams"

# Finally configure KeyVault firewall
Write-Host "Configuring KeyVault firewall"
az keyvault network-rule add --name $keyVaultName --ip-address $firewallIp | Out-Null
az keyvault update --name $keyVaultName --resource-group $resourceGroupName --default-action Deny | Out-Null

Write-Host "Public ip: $publicIp"
if ($null -ne $publicIp) {
    Write-Host "Removing firewall rule that allows access from current ip $publicIp"
    az keyvault network-rule remove --name $keyVaultName --ip-address $publicIp | Out-Null
}

Write-Host "✅  KeyVault firewall configured"

Write-Host
Write-Host "All application secrets rotated."
Write-Host "Done."
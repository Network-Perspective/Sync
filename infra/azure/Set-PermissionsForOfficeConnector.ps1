<#
.SYNOPSIS
    This script updates an Azure EntraId Applications to request the necessary permissions 

.NOTES
    - Prerequisites: Azure CLI 2.0 (https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
    - Usage:
        1. Login to Azure CLI
        2. Run the script
        3. Open the URL in the output to grant admin consent

.EXAMPLE
    ./Set-PermissionsForOfficeConnector.ps1
#>
Param(
)

# Constants
$appNameBasic = "Network Perspective Office 365 Sync"
$appNameWithTeams = "Network Perspective Office 365 Teams Sync"

function Update-Appliation 
{
    Param(
        [string]$appName,
        [bool]$teamsPermissions
    )
    
    Write-Host "Updating application '$appName'"

    # Check if app already exists
    $appId = az ad app list --display-name $appName --query "[].appId"  --output tsv

    if ($null -eq $appId) {
        # ask for confirmation to delete the app
        Write-Host "ERROR: Application '$appName' does not exists"
        exit 1
    }  

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

    Write-Host "✅ App permissions updated"
    Write-Host
    return $appId
}

Write-Host "Update EntraId applications -------------------------------------"
Write-Host
$appIdBasic     = Update-Appliation -appName $appNameBasic -teamsPermissions $false 
$appIdWithTeams = Update-Appliation -appName $appNameWithTeams -teamsPermissions $true

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
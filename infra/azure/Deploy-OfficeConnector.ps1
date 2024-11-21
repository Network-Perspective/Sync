<#
.SYNOPSIS
    This script creates an Azure EntraId Applications required for syncing Office 365 data with Network Perspective.

.EXAMPLE
    ./Deploy-OfficeConnectorKeyVault.ps1 
#>
Param(
    [Parameter()]
    [string] 
    $appNameBasic = "Network Perspective Office 365 Sync",

    [Parameter()]
    [string] 
    $appNameWithTeams = "Network Perspective Office 365 Teams Sync"
)

# Constants
$appCallbackUri = "https://app.networkperspective.io/sync/callback/office365"

# Function to generate a unique random looking, yet deterministic string
function Get-UniqueString ([string]$id, $length=13)
{
    $hashArray = (new-object System.Security.Cryptography.SHA512Managed).ComputeHash($id.ToCharArray())
        -join ($hashArray[1..$length] | ForEach-Object { [char]($_ % 26 + [byte][char]'a') })
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
    Write-Host
    return $appId
}

Write-Host "Registering EntraId applications -------------------------------------"
Write-Host
$appIdBasic     = Register-Appliation -appName $appNameBasic -keyVaultName $keyVaultName -teamsPermissions $false -secretPrefix "microsoft-client-basic"
$appIdWithTeams = Register-Appliation -appName $appNameWithTeams -keyVaultName $keyVaultName -teamsPermissions $true -secretPrefix "microsoft-client-with-teams"

# Print info about manual steps
Write-Host 
Write-Host "----------------------------------------------------------------------"
Write-Host "🔑🔑🔑  Save the following values in the keyvault  🔑🔑🔑"
Write-Host "microsoft-client-basic-id : $appIdBasic"
Write-Host "microsoft-client-with-teams-id : $appIdWithTeams"
Write-Host
Write-Host "Generate secrets for the above apps and save them in the keyvault"
Write-Host "microsoft-client-basic-secret"
Write-Host "az ad app credential reset --id $appIdBasic --display-name ""app-secret"""
Write-Host
Write-Host "microsoft-client-with-teams-secret"
Write-Host "az ad app credential reset --id $appIdWithTeams --display-name ""app-secret"""
Write-Host
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

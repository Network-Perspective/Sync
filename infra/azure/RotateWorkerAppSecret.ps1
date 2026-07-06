<#
.SYNOPSIS
    Rotates the worker Entra application secret and stores it in Azure Key Vault.

.DESCRIPTION
    This script locates the worker Key Vault created by deploy-worker.ps1, temporarily
    allows access from the current public IP address, resets the Entra application client
    secret, stores the new client secret in Key Vault, and removes the temporary firewall rule.

.PARAMETER entraAppName
    The display name of the Entra application. Default value is "Network Perspective Office 365 Sync".

.PARAMETER resourceGroupName
    The name of the resource group where the worker Key Vault is located. Default value is "RG-Np-Worker".

.PARAMETER keyVaultName
    Explicit Key Vault name override. If omitted, the script derives the worker Key Vault
    name from the resource group, matching deploy-worker.ps1.

.PARAMETER teamsPermissions
    Indicates whether the worker was deployed with Teams permissions enabled.
    This is only used as a fallback when the secret prefix cannot be inferred from Key Vault.

.EXAMPLE
    ./RotateWorkerAppSecrets.ps1

.EXAMPLE
    ./RotateWorkerAppSecrets.ps1 -keyVaultName "kv-np-example"
#>
Param(
    [Parameter()]
    [string] $entraAppName = "Network Perspective Office 365 Sync",

    [Parameter()]
    [string] $resourceGroupName = "RG-Np-Worker",

    [Parameter()]
    [string] $keyVaultName = "",

    [Parameter()]
    [bool] $teamsPermissions = $true
)

function Get-UniqueString ([string]$id, $length = 8)
{
    $hashArray = (New-Object System.Security.Cryptography.SHA512Managed).ComputeHash($id.ToCharArray())
        -join ($hashArray[1..$length] | ForEach-Object { [char]($_ % 26 + [byte][char]'a') })
}

function Resolve-SecretPrefix
{
    Param(
        [bool] $teamsPermissions
    )

    if ($teamsPermissions) {
        return "microsoft-client-with-teams"
    }

    return "microsoft-client-basic"
}

Write-Host "Script configuration -----------------------------------------------"
Write-Host "- Entra app name: $entraAppName"
Write-Host "- Resource group: $resourceGroupName"
if ([string]::IsNullOrWhiteSpace($keyVaultName)) {
    Write-Host "- KeyVault name: <derived from resource group>"
} else {
    Write-Host "- KeyVault name: $keyVaultName"
}
Write-Host "- Teams permissions: $teamsPermissions"
Write-Host

$publicIp = $null
$addedFirewallRule = $false
$resolvedKeyVaultName = $null

try {
    $resourceGroup = az group show --name $resourceGroupName --only-show-errors 2>$null | ConvertFrom-Json
    if ($null -eq $resourceGroup) {
        throw "Resource group '$resourceGroupName' not found"
    }

    if ([string]::IsNullOrWhiteSpace($keyVaultName)) {
        $resolvedKeyVaultName = "kv-np-$(Get-UniqueString($resourceGroup.id))"
        Write-Host "Locating KeyVault with derived name $resolvedKeyVaultName"
    } else {
        $resolvedKeyVaultName = $keyVaultName
        Write-Host "Locating KeyVault with explicit name $resolvedKeyVaultName"
    }

    $keyVault = az keyvault show --name $resolvedKeyVaultName --resource-group $resourceGroupName --only-show-errors 2>$null | ConvertFrom-Json
    if ($null -eq $keyVault) {
        throw "KeyVault '$resolvedKeyVaultName' not found in resource group '$resourceGroupName'"
    }

    Write-Host "✅ KeyVault found"
    Write-Host

    $publicIp = (Invoke-WebRequest -Uri "https://ifconfig.me/ip").Content.Trim()
    $existingIpRules = @(az keyvault show --name $resolvedKeyVaultName --resource-group $resourceGroupName --query "properties.networkAcls.ipRules[].value" --output tsv --only-show-errors 2>$null)

    if ($existingIpRules -contains $publicIp) {
        Write-Host "Current IP $publicIp is already allowed in KeyVault firewall"
    } else {
        Write-Host "Adding temporary KeyVault firewall rule for current IP $publicIp"
        az keyvault network-rule add --name $resolvedKeyVaultName --resource-group $resourceGroupName --ip-address $publicIp --only-show-errors | Out-Null
        $addedFirewallRule = $true

        Write-Host "Waiting for KeyVault firewall changes to propagate"
        Start-Sleep -Seconds 10
    }

    Write-Host
    Write-Host "Rotating secrets for Entra application -----------------------------"

    $applications = @(az ad app list --display-name $entraAppName --only-show-errors 2>$null | ConvertFrom-Json)
    if ($applications.Count -eq 0) {
        throw "Application '$entraAppName' not found"
    }

    if ($applications.Count -gt 1) {
        $appIds = ($applications | ForEach-Object { $_.appId }) -join ", "
        throw "More than one application matched '$entraAppName': $appIds"
    }

    $app = $applications[0]
    $secretPrefix = Resolve-SecretPrefix -teamsPermissions $teamsPermissions

    Write-Host "Updating application '$entraAppName'"
    $appSecret = az ad app credential reset --id $app.appId --display-name "app-secret" --only-show-errors 2>$null | ConvertFrom-Json

    if ($null -eq $appSecret -or [string]::IsNullOrWhiteSpace($appSecret.password)) {
        throw "Failed to generate a new secret for application '$entraAppName'"
    }

    Write-Host "Saving secrets in KeyVault"
    Write-Host "- app secret"
    az keyvault secret set --vault-name $resolvedKeyVaultName --name "$secretPrefix-secret" --value $appSecret.password --only-show-errors | Out-Null

    Write-Host
    Write-Host "✅ Application secret rotated"
    Write-Host "Done."
} catch {
    Write-Error $_
    exit 1
} finally {
    if ($addedFirewallRule -and -not [string]::IsNullOrWhiteSpace($publicIp) -and -not [string]::IsNullOrWhiteSpace($resolvedKeyVaultName)) {
        Write-Host
        Write-Host "Cleaning up KeyVault firewall -------------------------------------"
        az keyvault network-rule remove --name $resolvedKeyVaultName --resource-group $resourceGroupName --ip-address $publicIp --only-show-errors | Out-Null
        Write-Host "✅ KeyVault firewall cleaned up"
    }
}

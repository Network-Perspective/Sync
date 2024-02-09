#$env:VAULT_ADDR='https://c1.test.networkperspective.io:8200'

$namespace = 'default'
$excelServiceAccountName = 'np-sync-excel'
$mountPoint = "np-sync-excel-secrets"

# Assuming you've already authenticated with vault CLI 

# Create a policy that gives read-write access to this test secret
$policyHCL = @"
path "$mountPoint/*" {
  capabilities = ["create", "read", "update", "patch", "delete", "list"]
}
"@
$policyHCL | Out-File -Encoding ASCII np-sync-excel-policy.hcl

vault policy write np-sync-excel-policy np-sync-excel-policy.hcl

Remove-Item np-sync-excel-policy.hcl

# Create a role in Vault for Kubernetes auth
# This binds a Kubernetes service account to the created policy
# Adjust the bound_service_account_names and bound_service_account_namespaces as needed
vault write auth/kubernetes/role/excel-sync `
    bound_service_account_names=$excelServiceAccountName `
    bound_service_account_namespaces=$namespace `
    policies=np-sync-excel-policy `
    ttl=1h

Write-Output "Setup complete. The service account '$excelServiceAccountName' in the '$namespace' namespace has read-write access to $mountPoint/*."

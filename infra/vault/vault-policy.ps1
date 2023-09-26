$env:VAULT_ADDR='https://c1.test.networkperspective.io:8200'
# Assuming you've already authenticated with vault CLI and set VAULT_ADDR

# Enable Kubernetes authentication if it hasn't been
$vaultK8sAuthEnabled = vault auth list | Select-String "kubernetes/"
if (-not $vaultK8sAuthEnabled) {
    vault auth enable kubernetes
}

# Configure Kubernetes auth to communicate with the Kubernetes cluster
vault write auth/kubernetes/config kubernetes_host=https://kubernetes.default.svc:443    

# Create a test secret in Vault
vault kv put secret/test-key secret="This is a test value"

# Create a policy that gives read-write access to this test secret
$policyHCL = @"
path "secret/data/test-key" {
    capabilities = ["create", "update", "read", "delete"]
}
path "secret/metadata/test-key" {
    capabilities = ["list", "read", "delete"]
}
"@
$policyHCL | Out-File -Encoding ASCII test-policy.hcl

vault policy write test-policy test-policy.hcl

# Create a role in Vault for Kubernetes auth
# This binds a Kubernetes service account to the created policy
# Adjust the bound_service_account_names and bound_service_account_namespaces as needed
vault write auth/kubernetes/role/gsuite-sync `
    bound_service_account_names=chart-gsuite `
    bound_service_account_namespaces=default `
    policies=test-policy `
    ttl=1h

vault write auth/kubernetes/role/slack-sync `
    bound_service_account_names=chart-slack `
    bound_service_account_namespaces=default `
    policies=test-policy `
    ttl=1h

# Clean up
Remove-Item test-policy.hcl

Write-Output "Setup complete. The service account 'my-sa' in the 'default' namespace has read-write access to the test secret."

# $env:VAULT_ADDR='https://127.0.0.1:8200'
# $env:VAULT_ADDR="https://c1.test.networkperspective.io:8200"

# use -tls-skip-verify in staging environment

# if vault is already initialized, login
# vault login 

# enable audit
vault audit enable file file_path=/vault/audit/vault_audit.log
vault audit enable -path=stdout file file_path=stdout

# gsuite mount-point
vault secrets enable -path=np-sync-gsuite-secrets kv-v2

# slack mount-point
vault secrets enable -path=np-sync-slack-secrets kv-v2

# excel mount-point
vault secrets enable -path=np-sync-excel-secrets kv-v2

# Enable Kubernetes authentication if it hasn't been
$vaultK8sAuthEnabled = vault auth list | Select-String "kubernetes/"
if (-not $vaultK8sAuthEnabled) {
    vault auth enable kubernetes
}

# Configure Kubernetes auth to communicate with the Kubernetes cluster
vault write auth/kubernetes/config kubernetes_host=https://kubernetes.default.svc:443    
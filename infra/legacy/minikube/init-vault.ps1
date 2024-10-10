$env:VAULT_ADDR='http://127.0.0.1:8200'

vault login root

# add mount points
. $PSScriptRoot/../vault/vault-setup.ps1

# add policies
. $PSScriptRoot/../vault/slack-sync-vault-policy.ps1
. $PSScriptRoot/../vault/gsuite-sync-vault-policy.ps1
. $PSScriptRoot/../vault/excel-sync-vault-policy.ps1


if (Test-Path -Path "../secrets/hashing-key") {
    $hashingKey = Get-Content -Path "../secrets/hashing-key"
}
else {
    $hashingKey = "test"
}

# gsuite secrets
vault kv put -mount=np-sync-gsuite-secrets test-key secret=anything

if (Test-Path -Path "../secrets/google-key") {
    vault kv put -mount=np-sync-gsuite-secrets google-key secret="@../secrets/google-key"
}
else {
    vault kv put -mount=np-sync-gsuite-secrets google-key secret=test
}
vault kv put -mount=np-sync-gsuite-secrets hashing-key secret=$hashingKey

# slack secrets
vault kv put -mount=np-sync-slack-secrets test-key secret=anything
if (Test-Path -Path "../secrets/slack-client-id") {
    vault kv put -mount=np-sync-slack-secrets slack-client-id secret="@../secrets/slack-client-id"
}
else {
    vault kv put -mount=np-sync-slack-secrets slack-client-id secret=test
}

if (Test-Path -Path "../secrets/slack-client-secret") {
    vault kv put -mount=np-sync-slack-secrets slack-client-secret secret="@../secrets/slack-client-secret"
}
else {
    vault kv put -mount=np-sync-slack-secrets slack-client-secret secret=test
}
vault kv put -mount=np-sync-slack-secrets hashing-key secret=$hashingKey

# excel secrets
vault kv put -mount=np-sync-excel-secrets test-key secret=anything
vault kv put -mount=np-sync-excel-secrets hashing-key secret=$hashingKey

# $env:VAULT_ADDR='https://127.0.0.1:8200'
$env:VAULT_ADDR="https://c1.test.networkperspective.io:8200"

# use -tls-skip-verify in staging environment

# vault login
vault login

# gsuite secrets
vault kv put -mount=np-sync-gsuite-secrets test-key secret=anything
vault kv put -mount=np-sync-gsuite-secrets google-key secret="@../secrets/google-key"
vault kv put -mount=np-sync-gsuite-secrets hashing-key secret="@../secrets/hashing-key"

# slack secrets
vault kv put -mount=np-sync-slack-secrets test-key secret=anything
vault kv put -mount=np-sync-slack-secrets slack-client-id secret="@../secrets/slack-client-id"
vault kv put -mount=np-sync-slack-secrets slack-client-secret secret="@../secrets/slack-client-secret"
vault kv put -mount=np-sync-slack-secrets hashing-key secret="@../secrets/hashing-key"


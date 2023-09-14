$env:VAULT_ADDR='https://127.0.0.1:8200'
vault operator init  -tls-skip-verify
vault login -tls-skip-verify
vault secrets enable  -tls-skip-verify -path=secret kv-v2
vault kv put -tls-skip-verify secret/test-key secret=anything
vault kv put -tls-skip-verify secret/np-sync-gsuite-secrets @secrets/gsuite-secrets.json
vault kv put -tls-skip-verify secret/np-sync-slack-secrets @secrets/slack-secrets.json


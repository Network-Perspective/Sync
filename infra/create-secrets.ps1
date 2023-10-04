$namespace = "default"

# enable the following when not using vault
# kubectl create secret generic np-sync-gsuite-secrets --namespace $namespace --from-file=secrets/hashing-key --from-file=secrets/google-key
# kubectl create secret generic np-sync-slack-secrets --namespace $namespace --from-file=secrets/hashing-key --from-file=secrets/slack-client-id  --from-file=secrets/slack-client-secret
# ubectl create secret generic np-sync-rsa --namespace $namespace --from-file=secrets/key.pem --from-file=secrets/public.pem

kubectl create secret generic np-sync-mssql-sapassword --namespace "$namespace" --from-file=secrets/sapassword
kubectl create secret generic np-sync-domain-validation --namespace "$namespace" --from-file=clientSecret=secrets/domain-validation

# Read values from files
$acrLogin = Get-Content -Path "./secrets/acr-login"
$acrPassword = Get-Content -Path "./secrets/acr-password"

kubectl create secret docker-registry np-sync-np-acr --namespace $namespace --docker-server=networkperspective.azurecr.io --docker-username=$acrLogin --docker-password=$acrPassword

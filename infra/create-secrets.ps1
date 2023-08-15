$namespace = "default"

kubectl create secret generic np-sync-gsuite-secrets --namespace $namespace --from-file=secrets/hashing-key --from-file=secrets/google-key
kubectl create secret generic np-sync-slack-secrets --namespace $namespace --from-file=secrets/hashing-key --from-file=secrets/slack-client-id  --from-file=secrets/slack-client-secret
kubectl create secret generic np-sync-rsa --namespace $namespace --from-file=secrets/key.pem --from-file=secrets/public.pem

# Read values from files
$acrLogin = Get-Content -Path "./secrets/acr-login"
$acrPassword = Get-Content -Path "./secrets/acr-password"
$clientSecret = Get-Content -Path "./secrets/domain-validation"

kubectl create secret docker-registry np-acr-secret --namespace $namespace --docker-server=networkperspective.azurecr.io --docker-username=$acrLogin --docker-password=$acrPassword
kubectl create secret generic "domain-validation" --namespace $namespace --from-literal=clientSecret=$clientSecret 

$namespace = "default"

kubectl create secret generic np-sync-gsuite-secrets --from-file=secrets/hashing-key --from-file=secrets/google-key
kubectl create secret generic np-sync-slack-secrets --from-file=secrets/hashing-key --from-file=secrets/slack-client-id  --from-file=secrets/slack-client-secret
kubectl create secret generic np-sync-rsa --from-file=secrets/key.pem --from-file=secrets/public.pem

$acrLogin = Get-Content -Path "./secrets/acr-login"
$acrPassword = Get-Content -Path "./secrets/acr-password"

kubectl create secret docker-registry np-acr-secret --docker-server=networkperspective.azurecr.io --docker-username=$acrLogin --docker-password=$acrPassword

# Read values from files
$clientSecret = Get-Content -Path "./secrets/domain-validation.txt"
kubectl create secret generic "domain-validation" --namespace $namespace --from-literal=clientSecret=$clientSecret 

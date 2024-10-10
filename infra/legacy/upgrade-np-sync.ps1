$chartVersion = Get-Content -Path "./internal/chart-version"
$acrLogin = Get-Content -Path "./secrets/acr-login"
$acrPassword = Get-Content -Path "./secrets/acr-password"
$namespace = "default"

helm registry login "networkperspective.azurecr.io" --username $acrLogin --password $acrPassword

helm upgrade np-sync oci://networkperspective.azurecr.io/helm/np-sync --version $chartVersion --set mssql.acceptEula.value=Y -f ./secrets/production.yaml 

# recreate vault pod to update tls certificate
Write-Host "Restarting vault"
kubectl delete pod np-sync-vault-0 --namespace=$namespace
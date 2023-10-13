$chartVersion = Get-Content -Path "./internal/chart-version"
$acrLogin = Get-Content -Path "./secrets/acr-login"
$acrPassword = Get-Content -Path "./secrets/acr-password"

helm registry login "networkperspective.azurecr.io" --username $acrLogin --password $acrPassword
helm install np-sync oci://networkperspective.azurecr.io/helm/np-sync --version $chartVersion --set mssql.acceptEula.value=Y -f ./secrets/production.yaml
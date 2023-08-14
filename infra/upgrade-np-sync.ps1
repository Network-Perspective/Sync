$acrLogin = Get-Content -Path "./secrets/acr-login"
$acrPassword = Get-Content -Path "./secrets/acr-password"

helm registry login "networkperspective.azurecr.io" --username $acrLogin --password $acrPassword

helm upgrade np-sync oci://networkperspective.azurecr.io/helm/np-sync --version 2.1.2903 --set mssql.acceptEula.value=Y -f ./secrets/production.yaml
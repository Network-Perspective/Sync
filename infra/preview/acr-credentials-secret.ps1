$acrLogin = Get-Content -Path "./secrets/acr-login"
$acrPassword = Get-Content -Path "./secrets/acr-password"

kubectl create secret docker-registry np-acr-secret --docker-server=networkperspective.azurecr.io --docker-username=$acrLogin --docker-password=$acrPassword

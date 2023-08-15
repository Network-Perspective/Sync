#!/bin/bash

chartVersion=$(cat ./internal/chart-version)
acrLogin=$(cat ./secrets/acr-login)
acrPassword=$(cat ./secrets/acr-password)

helm registry login "networkperspective.azurecr.io" --username "$acrLogin" --password "$acrPassword"

helm install np-sync oci://networkperspective.azurecr.io/helm/np-sync --version "$chartVersion" --set mssql.acceptEula.value=Y -f ./secrets/staging.yaml

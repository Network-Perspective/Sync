#!/bin/bash

acrLogin=$(cat ./secrets/acr-login)
acrPassword=$(cat ./secrets/acr-password)

helm registry login "networkperspective.azurecr.io" --username "$acrLogin" --password "$acrPassword"

helm install np-sync oci://networkperspective.azurecr.io/helm/np-sync --version "2.1.2903" --set mssql.acceptEula.value=Y -f ./secrets/staging.yaml

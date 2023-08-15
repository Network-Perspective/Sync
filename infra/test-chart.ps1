helm lint helm-charts/np-sync
helm template helm-charts/np-sync --set mssql.acceptEula.value=Y --name-template=np-sync --debug > preview/np-sync.yaml
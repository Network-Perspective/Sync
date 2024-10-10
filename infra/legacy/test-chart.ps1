helm lint helm-charts/np-sync
helm template helm-charts/np-sync `
    --set mssql.acceptEula.value=Y `
    --set global.redeployOnUpdate=false `
    --set excel.enabled=true `
    --name-template=np-sync `
    --namespace=np-sync `
    --debug > preview/np-sync.yaml
helm lint helm-charts/np-worker

helm template helm-charts/np-worker `
    --set redeployOnUpdate=false `
    --name-template=np-worker `
    --namespace=np-worker `
    --debug > preview/np-worker.yaml
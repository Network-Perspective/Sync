# Kubernetes 

Installs or upgrades the `np-worker` chart

```powershell
helm upgrade --install np-worker oci://networkperspective.azurecr.io/helm/np-worker -f yourvalues.yaml
```

### test-chart.ps1
This script is used to lint and template the `np-worker` Helm chart. It helps in validating the chart and generating a Kubernetes manifest for preview.

### Create ACR Secret 
ACR login credentials must be available to the helm chart yo access Azure Container Registry (ACR). These credentials are shared by the vendor (Network Perspective) and can be saved to kubernetes with the following command.

```
kubectl create secret docker-registry np-sync-np-acr `
    --docker-server=networkperspective.azurecr.io `
    --docker-username=$username `
    --docker-password=$password `
    --namespace=$namespace
```    
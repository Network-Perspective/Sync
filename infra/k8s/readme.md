# Kubernetes 

Installs or upgrades the `np-worker` chart

```powershell
helm upgrade --install np-worker oci://networkperspective.azurecr.io/helm/np-worker -f yourvalues.yaml
```

### test-chart.ps1
This script is used to lint and template the `np-worker` Helm chart. It helps in validating the chart and generating a Kubernetes manifest for preview.


# helm install helm-charts/np-sync --set mssql.acceptEula.value=Y --name-template=np-sync --debug --dry-run > dry-run.yaml
#helm install --namespace np-sync helm-charts/np-sync --set mssql.acceptEula.value=Y --name-template=np-sync --debug --dry-run > dry-run.yaml
helm uninstall np-sync
#helm install helm-charts/np-sync ./mychart
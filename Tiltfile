# Tiltfile

# Define the Docker image gsuite service
docker_build('sync-gsuite', "./src", dockerfile='./src/GSuite.Dockerfile')
docker_build('sync-slack', "./src", dockerfile='./src/Slack.Dockerfile')
docker_build('sync-excel', "./src", dockerfile='./src/Excel.Dockerfile')


# Deploy the Helm chart
k8s_yaml(helm('infra/helm-charts/np-sync', name="np-sync", values='infra/minikube/tilt.yaml', set=[
    'mssql.acceptEula.value=Y',
    'mssql.edition.value=Developer',
    'gsuite.image.repository=sync-gsuite',
    'slack.image.repository=sync-slack',
    'excel.image.repository=sync-excel',
    'global.redeployOnUpdate=false'
]))

# Define resources
k8s_resource('np-sync-mssql', port_forwards='11433:1433', labels="infra")
k8s_resource('np-sync-vault', port_forwards='8200:8200', labels="infra")

# init vault after deployment
local_resource('vault-init', 'pwsh -file ./infra/minikube/init-vault.ps1',    
    resource_deps=['np-sync-vault'],
    labels="infra"
)

# connectors
k8s_resource('np-sync-gsuite', port_forwards='8081:8080', labels="connectors")
k8s_resource('np-sync-slack', port_forwards='8082:8080', labels="connectors")
k8s_resource('np-sync-excel', port_forwards='8083:8080', labels="connectors")

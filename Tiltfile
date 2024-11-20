# Tiltfile

# Define the Docker image for np-worker service
docker_build('np-worker', "./src", dockerfile='./src/Worker.Dockerfile', ignore=['./infra'])

# Deploy the Helm chart with the local image
k8s_yaml(helm('infra/helm-charts/np-worker', name="np-sync", 
    values=['infra/aws/values-tilt.yaml'], set=[
    'image.repository=np-worker',
    'image.pullPolicy=IfNotPresent',
    'global.redeployOnUpdate=false'
]))


# init vault after deployment
# local_resource('vault-init', 'pwsh -file ./infra/minikube/init-vault.ps1',    
#     resource_deps=['np-sync-vault'],
#     labels="infra"
# )

# connectors
# k8s_resource('np-sync-gsuite', port_forwards='8081:8080', labels="connectors", trigger_mode=TRIGGER_MODE_MANUAL)
# k8s_resource('np-sync-slack', port_forwards='8082:8080', labels="connectors", trigger_mode=TRIGGER_MODE_MANUAL)
# k8s_resource('np-sync-excel', port_forwards='8083:8080', labels="connectors", trigger_mode=TRIGGER_MODE_MANUAL)

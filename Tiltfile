# Tiltfile

# Define the Docker image gsuite service
docker_build('sync-gsuite', "./src", dockerfile='./src/GSuite.Dockerfile')
docker_build('sync-slack', "./src", dockerfile='./src/Slack.Dockerfile')


# Deploy the Helm chart
k8s_yaml(helm('infra/helm-charts/np-sync', set=[
    'mssql.acceptEula.value=Y',
    'mssql.sapassword=DAS2339dasjkl!##',
    'mssql.edition.value=Developer',
    #'mssql.edition.value=Express',
    'gsuite.image.repository=sync-gsuite',
    'slack.image.repository=sync-slack',
    'global.redeployOnUpdate=false',
    # 'vault.server.dev.enabled=true'
]))

# Define resources
k8s_resource('chart-mssql', port_forwards='11433:1433')
k8s_resource('chart-gsuite')
k8s_resource('chart-slack')
k8s_resource('chart-vault', port_forwards='8200:8200')
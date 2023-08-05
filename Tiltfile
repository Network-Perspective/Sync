# Tiltfile

# Define the Docker image gsuite service
docker_build('sync-gsuite', "./src", dockerfile='./src/GSuite.Dockerfile')

# Deploy the Helm chart
k8s_yaml(helm('infra/helm-charts/np-sync', set=[
    'mssql.acceptEula.value=Y',
    'mssql.sapassword=DAS2339dasjkl!##',
    'mssql.edition.value=Developer',
    #'mssql.edition.value=Express',
    'gsuite.image.repository=sync-gsuite',
]))

# Define resources
k8s_resource('chart-mssql', port_forwards='11433:1433')
k8s_resource('chart-gsuite', port_forwards='6001:80')
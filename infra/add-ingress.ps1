# helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
# helm repo update

Write-Host "Creating static IP Address for Ingress"
gcloud compute addresses create np-sync-ip --region europe-west4

$output = gcloud compute addresses describe np-sync-ip --region europe-west4 --format=json | ConvertFrom-Json
$ipAddress = $output.address
Write-Host "IP Address is: $ipAddress"

Write-Host "Installing Ingress"
helm install nginx-ingress ingress-nginx/ingress-nginx `
    --namespace ingress-nginx `
    --create-namespace `
    --set controller.service.loadBalancerIP=$ipAddress `
    --set controller.service.externalTrafficPolicy=Local `
    --set controller.image.allowPrivilegeEscalation=false

# helm install nginx-ingress ingress-nginx/ingress-nginx `
#     --set controller.service.loadBalancerIP=$ipAddress `
#     --set controller.service.externalTrafficPolicy=Local 


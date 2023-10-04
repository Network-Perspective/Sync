$allowedIps = "20.8.20.1/32"

# Add the ingress-nginx helm repository and update
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update

# Check if the nginx-ingress release is already installed
$existingRelease = helm list --namespace ingress-nginx --filter nginx-ingress | Select-String "nginx-ingress"
$helmCommand = "install"
if ($existingRelease) {
    $helmCommand = "upgrade"
    Write-Host "Upgrading Ingress"
} else {
    Write-Host "Installing Ingress"
}

# Install the Nginx Ingress using Helm with the specified Elastic IP
helm $helmCommand nginx-ingress ingress-nginx/ingress-nginx `
    --namespace ingress-nginx `
    --create-namespace `
    --set controller.service.externalTrafficPolicy=Local `
    --set controller.image.allowPrivilegeEscalation=false `
    --set controller.service.loadBalancerSourceRanges="{$allowedIps}" 
    

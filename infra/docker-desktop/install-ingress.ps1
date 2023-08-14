helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update

Write-Host "Installing Ingress"
helm install nginx-ingress ingress-nginx/ingress-nginx `
    --namespace ingress-nginx `
    --create-namespace `
    --set controller.service.externalTrafficPolicy=Local `
    --set controller.image.allowPrivilegeEscalation=false


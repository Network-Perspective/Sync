$currentContext = kubectl config current-context

if ($currentContext -ne "docker-desktop") {
    Write-Host "The current context is not Docker Desktop. Exiting..."
    exit
}

helm repo add jetstack https://charts.jetstack.io
helm repo update

helm install cert-manager jetstack/cert-manager `
    --namespace cert-manager `
    --create-namespace `
    --version v1.12.0 `
    --set installCRDs=true `
    --set resources.requests.cpu=50m `
    --set resources.requests.memory=32Mi
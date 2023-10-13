# Variables
$namespace = "default"
$secretName = "domain-validation"

# Read values from files
$clientSecret = Get-Content -Path "./secrets/$secretName.txt"

# Delete the existing secret if it exists
$secretExists = kubectl get secret $secretName --namespace $namespace -o jsonpath='{.metadata.name}' 2>$null
if ($secretExists) {
    kubectl delete secret $secretName --namespace $namespace
}

# Create a Kubernetes Secret with all the required values
kubectl create secret generic $secretName --namespace $namespace `
    --from-literal=clientSecret=$clientSecret 

# Confirm the Secret is created
kubectl get secret $secretName --namespace $namespace

# Output confirmation
Write-Host "Kubernetes Secret created successfully!"

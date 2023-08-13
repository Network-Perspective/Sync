# helm repo add jetstack https://charts.jetstack.io
# helm repo update

kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.12.0/cert-manager.crds.yaml

helm install cert-manager jetstack/cert-manager --namespace cert-manager --create-namespace --version v1.12.0  -f ./secrets/cert-manager-values.yaml

# gke autopilot
#helm install cert-manager jetstack/cert-manager --namespace cert-manager --create-namespace --version v1.12.0 --set global.leaderElection.namespace=cert-manager --set global.resources.requests.cpu=10m --set global.resources.requests.memory=32Mi
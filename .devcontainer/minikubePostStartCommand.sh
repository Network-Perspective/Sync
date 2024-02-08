#!/bin/bash

# start minikube
minikube start

# install ingress
minikube addons enable ingress
minikube addons enable metrics-server

# install cert-manager
helm upgrade --install cert-manager jetstack/cert-manager \
    --namespace cert-manager \
    --create-namespace \
    --version v1.12.0 \
    --set installCRDs=true \
    --set resources.requests.cpu=50m \
    --set resources.requests.memory=32Mi

kubectl create secret generic np-sync-mssql-sapassword --namespace "default" --from-literal=sapassword="P@ssw0rd" --save-config --dry-run=client -o yaml | kubectl apply -f -

# forward ports to localhost
# nohup socat TCP-LISTEN:80,fork TCP:$(minikube ip):80 &
# nohup socat TCP-LISTEN:443,fork TCP:$(minikube ip):443 &

# nohup kubectl port-forward service/np-sync-vault 8200:8200 &

export VAULT_ADDR='http://127.0.0.1:8200'

echo "Run 'tilt up' to start the application"
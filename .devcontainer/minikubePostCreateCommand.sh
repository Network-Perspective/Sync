#!/bin/bash

# install vault cli
# sudo apt update && sudo apt install gpg wget -y
wget -O- https://apt.releases.hashicorp.com/gpg | sudo gpg --dearmor -o /usr/share/keyrings/hashicorp-archive-keyring.gpg
# gpg --no-default-keyring --keyring /usr/share/keyrings/hashicorp-archive-keyring.gpg --fingerprint
echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/hashicorp-archive-keyring.gpg] https://apt.releases.hashicorp.com $(lsb_release -cs) main" | sudo tee /etc/apt/sources.list.d/hashicorp.list
sudo apt update && sudo apt install vault -y

# install socat
sudo apt install socat -y  

# start minikube
minikube start

# install ingress
minikube addons enable ingress

# install cert-manager
helm repo add jetstack https://charts.jetstack.io
helm repo update

helm upgrade --install cert-manager jetstack/cert-manager \
    --namespace cert-manager \
    --create-namespace \
    --version v1.12.0 \
    --set installCRDs=true \
    --set resources.requests.cpu=50m \
    --set resources.requests.memory=32Mi

# forward ports to localhost
nuhup socat TCP-LISTEN:80,fork TCP:$(minikube ip):80 &
nuhup socat TCP-LISTEN:443,fork TCP:$(minikube ip):443 &

#!/bin/bash

# Check if jq is installed
if ! command -v jq &> /dev/null; then
    echo "Error: jq is not installed. Please install jq and try again."
    exit 1
fi

helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update

echo "Creating static IP Address for Ingress"
gcloud compute addresses create np-sync-ip --region europe-west4

ipAddress=$(gcloud compute addresses describe np-sync-ip --region europe-west4 --format=json | jq -r '.address')
echo "IP Address is: $ipAddress"

echo "Installing Ingress"
helm install nginx-ingress ingress-nginx/ingress-nginx \
    --namespace ingress-nginx \
    --create-namespace \
    --set controller.service.loadBalancerIP="$ipAddress" \
    --set controller.service.externalTrafficPolicy=Local \
    --set controller.image.allowPrivilegeEscalation=false

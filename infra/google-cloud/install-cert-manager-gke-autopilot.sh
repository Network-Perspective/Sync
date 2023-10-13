#!/bin/bash

helm repo add jetstack https://charts.jetstack.io
helm repo update

helm install cert-manager jetstack/cert-manager \
    --namespace cert-manager --create-namespace \
    --version v1.12.0 \
    --set installCRDs=true \
    --set global.leaderElection.namespace=cert-manager \
    --set resources.requests.cpu=50m \
    --set resources.requests.memory=32Mi

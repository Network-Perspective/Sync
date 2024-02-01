#!/bin/bash

# forward ports to localhost
nohup socat TCP-LISTEN:80,fork TCP:$(minikube ip):80 &
nohup socat TCP-LISTEN:443,fork TCP:$(minikube ip):443 &

nohup kubectl port-forward service/np-sync-vault 8200:8200 &
$env:VAULT_ADDR='http://127.0.0.1:8200'
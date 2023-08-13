# helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
# helm repo update
helm install nginx-ingress ingress-nginx/ingress-nginx

#create ips
gcloud compute addresses create np-sync-ip --region europe-west4
gcloud compute addresses describe np-sync-ip --region europe-west4

helm install nginx-ingress ingress-nginx/ingress-nginx --namespace default --set controller.service.loadBalancerIP=34.90.221.95 --set controller.service.externalTrafficPolicy=Local

#$ helm install --name hellopapp-nginx-ingress stable/nginx-ingress --set rbac.create=true --set controller.service.externalTrafficPolicy=Local
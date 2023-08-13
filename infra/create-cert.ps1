#openssl genrsa -out secrets/key.pem 4096
#openssl rsa -in secrets/key.pem -outform PEM -pubout -out secrets/public.pem
kubectl create secret generic np-sync-rsa --from-file=secrets/key.pem --from-file=secrets/public.pem

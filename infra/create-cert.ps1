#openssl req -new -newkey rsa:2048 -nodes -x509 -keyout sync.key -out sync.crt
openssl genrsa -out secrets/key.pem 4096
openssl rsa -in secrets/key.pem -outform PEM -pubout -out secrets/public.pem
kubectl create secret generic np-sync-rsa2 --from-file=secrets/key.pem --from-file=secrets/public.pem

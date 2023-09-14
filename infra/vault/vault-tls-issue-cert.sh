export VAULT_K8S_NAMESPACE="default" 
export VAULT_HELM_RELEASE_NAME="vault" 
export VAULT_SERVICE_NAME="vault-internal" 
export K8S_CLUSTER_NAME="cluster.local" 
export WORKDIR=./secrets

# kubectl delete csr vault.svc

kubectl create -f ${WORKDIR}/csr.yaml

kubectl certificate approve vault.svc

kubectl get csr vault.svc

kubectl get csr vault.svc -o jsonpath='{.status.certificate}' | openssl base64 -d -A -out ${WORKDIR}/vault.crt

kubectl config view \
--raw \
--minify \
--flatten \
-o jsonpath='{.clusters[].cluster.certificate-authority-data}' \
| base64 -d > ${WORKDIR}/vault.ca

kubectl create secret generic np-sync-vault-tls \
   -n $VAULT_K8S_NAMESPACE \
   --from-file=tls.key=${WORKDIR}/vault.key \
   --from-file=tls.crt=${WORKDIR}/vault.crt \
   --from-file=ca.crt=${WORKDIR}/vault.ca

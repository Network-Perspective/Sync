# Google cloud - step by step deployement

## 1. Create project
<details>
  <summary>Use project from local CLI</summary>    
    gcloud config set project network-perspective-sync

</details>
## 2. Enable kubernetes API
## 3. Create cluster
### Standard cluster (with confidential nodes)
Use default configurate with the following changes:
* Deault node pool 
  * number of nodes = 1
  * n2d-standard-4 
     * 4 vcpu (2 core), 16gb ram
* Security
  * Enable confidential GKE nodes  

Estimated monthly cost	$192

<details>
  <summary>CLI command</summary>    

```
gcloud beta container --project "network-perspective-sync" clusters create "np-sync-cluster" --zone "europe-west1-b" --no-enable-basic-auth --cluster-version "1.27.3-gke.100" --release-channel "regular" --machine-type "n2d-standard-4" --image-type "COS_CONTAINERD" --disk-type "pd-balanced" --disk-size "100" --metadata disable-legacy-endpoints=true --scopes "https://www.googleapis.com/auth/devstorage.read_only","https://www.googleapis.com/auth/logging.write","https://www.googleapis.com/auth/monitoring","https://www.googleapis.com/auth/servicecontrol","https://www.googleapis.com/auth/service.management.readonly","https://www.googleapis.com/auth/trace.append" --num-nodes "1" --logging=SYSTEM,WORKLOAD --monitoring=SYSTEM --enable-ip-alias --network "projects/network-perspective-sync/global/networks/default" --subnetwork "projects/network-perspective-sync/regions/europe-west1/subnetworks/default" --no-enable-intra-node-visibility --default-max-pods-per-node "110" --security-posture=standard --workload-vulnerability-scanning=disabled --no-enable-master-authorized-networks --addons HorizontalPodAutoscaling,HttpLoadBalancing,GcePersistentDiskCsiDriver --enable-autoupgrade --enable-autorepair --max-surge-upgrade 1 --max-unavailable-upgrade 0 --binauthz-evaluation-mode​=DISABLED --enable-managed-prometheus --enable-shielded-nodes --enable-confidential-nodes --node-locations "europe-west1-b"
```
</details>

### Autopilot
All defaults are fine, however autopilot cluster do not support confidential nodes.
<details>
  <summary>CLI command</summary>    

```
gcloud container --project "network-perspective-sync" clusters create-auto "np-sync-autopilot-cluster" --region "europe-west1" --release-channel "regular" --network "projects/network-perspective-sync/global/networks/default" --subnetwork "projects/network-perspective-sync/regions/europe-west1/subnetworks/default" --cluster-ipv4-cidr "/17"
```
</details>

## 4. Configure KMS for Vault autounseal
### Enable Cloud Key Management Service (KMS) API
### Create a KMS Keyring and Key:
Navigate to the Cryptographic Keys page in the Cloud Console.
Create a Keyring and then a cryptographic key.

```
gcloud kms keyrings create vault-keyring --location global
gcloud kms keys create unseal-key --location global --keyring vault-keyring --purpose encryption
```

### Create a GCP Service Account for Vault:

```
gcloud iam service-accounts create np-sync-vault --display-name "Np Sync Vault"
```

### Grant the necessary permissions to the service account:

```
gcloud kms keys add-iam-policy-binding unseal-key \
  --location global \
  --keyring vault-keyring \
  --member serviceAccount:np-sync-vault@network-perspective-sync.iam.gserviceaccount.com \
  --role roles/cloudkms.cryptoKeyEncrypterDecrypter,

gcloud kms keys add-iam-policy-binding unseal-key \
  --location global \
  --keyring vault-keyring \
  --member serviceAccount:np-sync-vault@network-perspective-sync.iam.gserviceaccount.com \
  --role roles/cloudkms.viewer
```

### Get the credentials for the service account:

```
gcloud iam service-accounts keys create ./secrets/np-sync-vault-sa.json --iam-account np-sync-vault@network-perspective-sync.iam.gserviceaccount.com
```

### Create a Kubernetes Secret from the service account credentials:

Connect to your cluser if you haven't before
```
gcloud container clusters get-credentials np-sync-cluster --zone europe-west1-b --project network-perspective-sync
kubectl config use-context gke_network-perspective-sync_europe-west1-b_np-sync-cluster
```
Create k8s secret
```
kubectl create secret generic np-sync-vault-sa --from-file=./secrets/np-sync-vault-sa.json
```

Delete the local copy of credentials file
```
rm ./secrets/np-sync-vault-sa.json
```

### For next steps refer to [Deployment docs](../readme.md)


### Lastly limit source ips that can access the ingress 
```
upgrade-ingress-limit-source-ip.ps1
```
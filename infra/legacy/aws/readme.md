# AWS - step by step deployement

## Create cluster

```
eksctl create cluster -f np-sync-cluster.yaml 
```

### EBS driver
We'll need persisten storage, hence a driver to provision ebs (elastic block store) volumes configured as described in [offical docs](https://docs.aws.amazon.com/eks/latest/userguide/ebs-csi.html) or with instruction belowbelow:


<details>
  <summary>Configure EBS driver step by step instruction</summary>     

#### enable oidc provider for the cluster

```
eksctl utils associate-iam-oidc-provider --region=eu-central-1 --cluster=np-sync-cluster --approve

```
#### Create service account for ebs driver
```
eksctl create iamserviceaccount `
    --name ebs-csi-controller-sa `
    --namespace kube-system `
    --cluster np-sync-cluster `
    --role-name AmazonEKS_EBS_CSI_DriverRole `
    --role-only `
    --attach-policy-arn arn:aws:iam::aws:policy/service-role/AmazonEBSCSIDriverPolicy `
    --approve

```
#### Get arn of the created account
```
aws iam get-role --role-name AmazonEKS_EBS_CSI_DriverRole --query 'Role.Arn' --output text

```
#### Install ebs driver
```
eksctl create addon --name aws-ebs-csi-driver --cluster np-sync-cluster --service-account-role-arn arn:aws:iam::111122223333:role/AmazonEKS_EBS_CSI_DriverRole --force
```
</details>

## Connect kubectl
Configure kubectl to access created cluster so we can deploy charts.
```
eksctl utils write-kubeconfig --name np-sync-cluster --region eu-central-1

kubectl config use-context sync-cluster@np-sync-cluster.eu-central-1.eksctl.io
```

## Install cert manager
Cert Manager is a native Kubernetes certificate management controller. Connectors use it to issue Let's Encrypt ssl certificates using domain validation. This dependency is not strictly necessary, if other way of providing ssl certificates is available. A script to install cert-manager is provided. 
```
.\aws\install-cert-manager.ps1
```

## Install ingress
Helm chart out of the box supports nginx ingress controller. 
Refer to [this guide](https://kubernetes.github.io/ingress-nginx/deploy/) for more details. The installation script will configure ingress with a static ip address to properly bind a domain name.
```
.\aws\install-ingress.ps1
```

## Create secrets
This will create basic k8s secrets, without the api keys that will be later stored in hcp vault
```
.\create-secrets.ps1
```
### HCPVault autounseal key
HCP Vault needs a key to autounseal, the script creates a key and configures iam account that can access it.
```
.\aws\create-aws-unseal-secret.ps1
```

## Install np sync
An install script that deploys helm chart provided. It will install app and hcp vault as a subchart.
```
.\install-np-sync.ps1
```

## Configure HCPVault
`Google service account keys`, `slack client id`, `slack client secret`, `hashing-key` should be stored securely in Vault. HCP Vault can be deployed as a subchart, if so first initialize Vault and configure autounseal. Then securely deposit secrets inside the Vault. Please refer to scripts in [./infra/valut](valut) folder for setting up Vault and creating secrets.

* `vault-connect.ps1` - port forward vault to localhost
* `vault-setup.ps1` - initialize & configure audit & mount points
* `vault-secrets.ps1` - create secrets
* `gsuite-sync-vault-policy.ps1` - add access policy for gsuite connector
* `slack-sync-vault-policy.ps1` - add access policy for slack connector

## Test
Access `https://slack.yourid.sync.networkperspective.io/health` and
`https://gsuite.yourid.sync.networkperspective.io/health` to check service health

## Firewall
This will limit access to connectors to single source ip address. 
```
upgrade-ingress-limit-source-ip.ps1
```

# Cleanup
To delete the cluster and everything inside:

<details>
  <summary>Cleanup all resources</summary>    

```
eksctl delete iamserviceaccount `
     --name ebs-csi-controller-sa `
     --namespace kube-system `
     --cluster np-sync-cluster 

eksctl delete cluster -f np-sync-cluster.yaml --disable-nodegroup-eviction  
```

<details>
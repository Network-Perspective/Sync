# gcloud container clusters describe autopilot-cluster-1 --region europe-west4 --project norse-strata-348517 
#--format="value(privateClusterConfig.masterIpv4CidrBlock)"


# 10.58.0.0/17

gcloud compute firewall-rules create allow-master-to-pods-8443 `
    --direction=INGRESS `
    --priority=1000 `
    --network=default `
    --action=ALLOW `
    --rules=tcp:8443 `
    --source-ranges=10.58.0.0/17

gcloud compute --project=norse-strata-348517 firewall-rules create allow-master-to-pods-8443 `
    --direction=INGRESS --priority=1000 --network=default --action=ALLOW --rules=tcp:8433 --source-ranges=10.58.0.0/17
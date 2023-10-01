$allowedIps = "20.8.20.1/32"

$output = gcloud compute addresses describe np-sync-ip --region europe-west1 --format=json | ConvertFrom-Json
$ipAddress = $output.address
Write-Host "Ingress ip address is: $ipAddress"

Write-Host "Whitelisting ips $allowedIps"
helm upgrade nginx-ingress ingress-nginx/ingress-nginx `
    --namespace ingress-nginx `
    --create-namespace `
    --set controller.service.loadBalancerIP=$ipAddress `
    --set controller.service.loadBalancerSourceRanges="{$allowedIps}" `
    --set controller.service.externalTrafficPolicy=Local `
    --set controller.image.allowPrivilegeEscalation=false


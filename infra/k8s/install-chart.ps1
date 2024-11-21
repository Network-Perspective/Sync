# using local aws values, customize with your own values
helm upgrade --install np-worker oci://networkperspective.azurecr.io/helm/np-worker -f ../aws/values-tilt.yaml 

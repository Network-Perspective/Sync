# user test registry and local aws values
helm upgrade --install np-worker oci://testnetworkperspective.azurecr.io/helm/np-worker -f ../aws/values-tilt.yaml --set image.repository=testnetworkperspective.azurecr.io/connectors/worker
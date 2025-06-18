# Network Perspective Worker Azure Deployment

## Overview

This deployment solution automates the provisioning of Network Perspective Worker as an Azure Container Instance with secure network configuration and KeyVault integration. The PowerShell deployment script creates all necessary resources based on a YAML configuration file.

The deployment includes:

1. **Virtual Network** with NAT Gateway for outbound internet access
2. **Azure KeyVault** with network security (accessible only from the container subnet)
3. **Container Instance** running the worker with a managed identity to access KeyVault
4. **EntraID Application** registration with required API permissions

## Prerequisites

Before deployment, ensure you have:

- **Azure CLI** (v2.0+) installed and logged in with sufficient permissions
- **PowerShell** with the PowerShell-YAML module (automatically installed if missing)
- Access to the Network Perspective Azure Container Registry
- **Global Administrator** or **Application Administrator** role in your Microsoft Entra ID tenant (for app registration)

The script was tested from Azure Cloud Shell that should meet the requirements by default.

## Files

- `deploy-worker.ps1`: The main PowerShell deployment script
- `configuration.yaml`: YAML configuration file for all deployment parameters
- `np-worker.bicep`: The Bicep template defining the infrastructure resources

## Configuration

The `configuration.yaml` file contains all settings required for the deployment:

```yaml
configuration:  
  resourceGroupName: "RG-Np-Worker"      # Azure resource group name (created if it doesn't exist)
  region: "eastus"                       # Azure region for deployment

  # Container registry settings
  registryUsername: "placeholder"        # Username for Network Perspective container registry
  registryPassword: "placeholder"        # Password for Network Perspective container registry
  containerImageTag: "latest"            # Container image tag to deploy

  # EntraID application settings
  entraAppName: "Network Perspective Office 365 Sync"  # Name for the registered application
  entraAppCallbackUri: "https://app.networkperspective.io/sync/callback/office365"  # Callback URL

  # Application settings
  networkPerspectiveApiUrl: "https://app.networkperspective.io/" # NP API endpoint
  applicationInsightsConnectionString: "placeholder"            # Optional App Insights
  applicationInsightsRoleInstance: "placeholder"                # Optional App Insights
    
  teamsPermissions: true  # Whether to include Teams API permissions for the app

# KeyVault secrets to be created
keyvault:    
  orchestrator-client-secret: "placeholder"  # Client secret for the orchestrator
  orchestrator-client-name: "placeholder"     # Client ID for the orchestrator
```

Replace placeholder values with your actual configuration settings before deployment.

## Deployment Instructions

1. **Configure the YAML File**

   Edit `configuration.yaml` to set all required parameters as described above.

2. **Login to Azure CLI**

   ```powershell
   az login
   ```

3. **Run the Deployment Script**

   ```powershell
   ./deploy-worker.ps1 -configurationFile "./configuration.yaml"
   ```

4. **Grant Admin Consent**

   After deployment completes, you'll need to grant admin consent for the registered EntraID application from the Network Perspective Admin panel.

## Deployed Resources

The deployment script creates the following Azure resources:

### Core Infrastructure
- **Resource Group**: Creates if it doesn't exist (specified in configuration)
- **Virtual Network**: Address space 10.0.0.0/16 with two subnets:
  - Container Subnet: 10.0.0.0/24 with ACI delegation and KeyVault service endpoint
  - KeyVault Subnet: 10.0.1.0/24 
- **NAT Gateway**: Provides outbound internet access for the container

### Security & Identity
- **Azure KeyVault**: Secured with RBAC authorization and network ACLs
- **User-Assigned Managed Identity**: Used by the container to access KeyVault
- **KeyVault Role Assignment**: Grants the container identity access to KeyVault secrets
- **EntraID Application**: Registered with required Microsoft Graph API permissions

### Container Instance
- **Container Group**: Deployed in private VNet with managed identity
- **Container Configuration**: 
  - Image: networkperspective.azurecr.io/connectors/worker:<tag>
  - Resources: 1 vCPU, 1 GB RAM
  - Network: Private subnet with NAT Gateway for outbound traffic

## Resource Security

The deployment implements several security best practices:

- **Managed Identity**: Container uses a user-assigned managed identity rather than stored credentials
- **KeyVault RBAC**: Uses modern RBAC authorization instead of access policies 
- **Network Security**: KeyVault is only accessible from the container subnet via service endpoint
- **Secure Environment Variables**: Sensitive settings are stored in KeyVault, not in container configuration

## Accessing Deployed Resources

After deployment, the following outputs are available from the Bicep template:

- **containerGroupId**: The resource ID of the deployed container group
- **containerIPAddress**: The private IP address of the container
- **keyVaultUri**: The URI of the deployed KeyVault

To access the container logs:

```powershell
az container logs --resource-group "RG-Np-Worker" --name "np-worker-<uniqueId>-cg"
```

Where `<uniqueId>` is the generated unique identifier for your deployment.

## Troubleshooting

The script is idempotent and will not fail if the resources already exist. Should it fail please try to run it again and share the error message with the Network Perspective support team.

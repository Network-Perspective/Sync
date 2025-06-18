// Network Perspective Worker Container Instance and KeyVault Deployment
// This template deploys:
// 1. Virtual Network with NAT Gateway for outbound internet access
// 2. Azure KeyVault (private access only)
// 3. Container Instance running worker image with access to KeyVault

@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Container image tag')
param containerImageTag string = 'latest'

@description('Container registry username')
param registryUsername string

@description('Container registry password')
// this is just readonly permission to pull the container image 
#disable-next-line secure-secrets-in-params
param registryPassword string

@description('Network Perspective API base URL')
param networkPerspectiveApiUrl string = 'https://app.networkperspective.io'

// Optional environment variables
@description('Application Insights Connection String')
param applicationInsightsConnectionString string = ''

@description('Application Insights Role Instance')
param applicationInsightsRoleInstance string = ''

// Network settings
@description('Virtual Network Address Space')
param vnetAddressPrefix string = '10.0.0.0/16'

@description('Container Subnet Address Space')
param containerSubnetAddressPrefix string = '10.0.0.0/24'

@description('KeyVault Subnet Address Space')
param keyVaultSubnetAddressPrefix string = '10.0.1.0/24'

@description('Unique ID for resource naming')
param uniqueId string 

@description('KeyVault name')
param keyVaultName string = 'np-worker-${uniqueId}-kv'

// Resource naming
var baseName = 'np-worker-${uniqueId}'
var vnetName = '${baseName}-vnet'
var natGatewayName = '${baseName}-natgw'
var containerGroupName = '${baseName}-cg'
var containerSubnetName = 'container-subnet'
var keyVaultSubnetName = 'keyvault-subnet'
var containerManagedIdentityName = '${baseName}-identity'

// Deploy Virtual Network with two subnets and NAT Gateway
resource natGateway 'Microsoft.Network/natGateways@2022-11-01' = {
  name: natGatewayName
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    idleTimeoutInMinutes: 4
    publicIpAddresses: [
      {
        id: natPublicIP.id
      }
    ]
  }
}

resource natPublicIP 'Microsoft.Network/publicIPAddresses@2022-11-01' = {
  name: '${natGatewayName}-pip'
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    publicIPAllocationMethod: 'Static'
  }
}

resource vnet 'Microsoft.Network/virtualNetworks@2022-11-01' = {
  name: vnetName
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        vnetAddressPrefix
      ]
    }
    subnets: [
      {
        name: containerSubnetName
        properties: {
          addressPrefix: containerSubnetAddressPrefix
          delegations: [
            {
              name: 'aci-delegation'
              properties: {
                serviceName: 'Microsoft.ContainerInstance/containerGroups'
              }
            }
          ]
          natGateway: {
            id: natGateway.id
          }
          serviceEndpoints: [
            {
              service: 'Microsoft.KeyVault'
              locations: ['*']
            }
          ]
        }
      }
      {
        name: keyVaultSubnetName
        properties: {
          addressPrefix: keyVaultSubnetAddressPrefix
        }
      }
    ]
  }
}

// Create User-assigned managed identity for container instance
resource containerIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: containerManagedIdentityName
  location: location
}

// Deploy KeyVault with private access only using RBAC authorization
resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true  // Changed to use RBAC authorization
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    // Access policies removed as they're not used with RBAC
    networkAcls: {
      defaultAction: 'Deny'
      bypass: 'AzureServices'
      ipRules: []
      virtualNetworkRules: [
        {
          id: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, containerSubnetName)
        }
      ]
    }
  }
  dependsOn: [
    vnet
  ]
}

// Add Key Vault Secrets Officer role assignment for the container managed identity
resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, containerIdentity.id, 'Key Vault Secrets Officer')
  scope: keyVault
  properties: {
    principalId: containerIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7') // Key Vault Secrets Officer built-in role
  }
}

// Deploy Container Instance with private network
resource containerGroup 'Microsoft.ContainerInstance/containerGroups@2023-05-01' = {
  name: containerGroupName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${containerIdentity.id}': {}
    }
  }
  properties: {
    containers: [
      {
        name: 'np-worker'
        properties: {
          image: 'networkperspective.azurecr.io/connectors/worker:${containerImageTag}'
          environmentVariables: concat([
            {
              name: 'Infrastructure__Core__BaseUrl'
              value: networkPerspectiveApiUrl
            }
            {
              name: 'Infrastructure__Orchestrator__BaseUrl'
              value: networkPerspectiveApiUrl
            }
            {              
              name: 'Infrastructure__Vaults__AzureKeyVault__BaseUrl'
              // Use public KeyVault URL - access is allowed via network ACL from container subnet
              value: keyVault.properties.vaultUri
            }
          ],
          // Conditionally add Application Insights settings if provided
          !empty(applicationInsightsConnectionString) ? [
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: applicationInsightsConnectionString
            }
          ] : [],
          !empty(applicationInsightsRoleInstance) ? [
            {
              name: 'ApplicationInsights__RoleInstance'
              value: applicationInsightsRoleInstance
            }
          ] : [])
          resources: {
            requests: {
              cpu: 1
              memoryInGB: 1
            }
          }
        }
      }
    ]
    osType: 'Linux'
    restartPolicy: 'Always'
    imageRegistryCredentials: [
      {
        server: 'networkperspective.azurecr.io'
        username: registryUsername
        // this is just readonly permission for the container to pull the image
        #disable-next-line secure-parameters
        password: registryPassword
      }
    ]
    subnetIds: [
      {
        id: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, containerSubnetName)
      }
    ]
  }
  // Container has no external dependencies
}

// Outputs
output containerGroupId string = containerGroup.id
output containerIPAddress string = containerGroup.properties.ipAddress.ip
output keyVaultUri string = keyVault.properties.vaultUri

param appName string
param environment string
param location string
param appServicePlanId string
param appInsightsConnectionString string
param keyVaultUri string
param subnetId string
param agentIpAddress string = ''
param aadTenantId string = ''
param aadClientId string = ''

var apiAppName = 'api-${appName}-${environment}'

var scmIpRestrictions = agentIpAddress != '' ? [
  {
    ipAddress: '${agentIpAddress}/32'
    action: 'Allow'
    priority: 100
    name: 'Allow deploy agent'
  }
  {
    ipAddress: 'Any'
    action: 'Deny'
    priority: 2147483647
    name: 'Deny all'
  }
] : [
  {
    ipAddress: 'Any'
    action: 'Deny'
    priority: 2147483647
    name: 'Deny all'
  }
]

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: apiAppName
  location: location
  tags: {
    application: appName
    environment: environment
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    virtualNetworkSubnetId: subnetId
    siteConfig: {
      use32BitWorkerProcess: false
      ftpsState: 'Disabled'
      netFrameworkVersion: 'v10.0'
      scmIpSecurityRestrictions: scmIpRestrictions
      scmIpSecurityRestrictionsDefaultAction: 'Deny'
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'KeyVaultUri'
          value: keyVaultUri
        }
        {
          name: 'AzureAd__Instance'
          value: az.environment().authentication.loginEndpoint
        }
        {
          name: 'AzureAd__TenantId'
          value: aadTenantId
        }
        {
          name: 'AzureAd__ClientId'
          value: aadClientId
        }
        {
          name: 'AzureAd__Audience'
          value: 'api://${aadClientId}'
        }
      ]
    }
  }
}

output webAppName string = webApp.name
output webAppPrincipalId string = webApp.identity.principalId

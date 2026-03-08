param appName string
param environment string
param location string

var vnetName = 'vnet-${appName}-${environment}'
var funcSubnetName = 'snet-func-${appName}-${environment}'
var apiSubnetName = 'snet-api-${appName}-${environment}'

resource vnet 'Microsoft.Network/virtualNetworks@2023-09-01' = {
  name: vnetName
  location: location
  tags: {
    application: appName
    environment: environment
  }
  properties: {
    addressSpace: {
      addressPrefixes: ['10.0.0.0/16']
    }
    subnets: [
      {
        name: funcSubnetName
        properties: {
          addressPrefix: '10.0.1.0/24'
          delegations: [
            {
              name: 'delegation'
              properties: {
                serviceName: 'Microsoft.Web/serverFarms'
              }
            }
          ]
          serviceEndpoints: [
            { service: 'Microsoft.KeyVault' }
            { service: 'Microsoft.Sql' }
          ]
        }
      }
      {
        name: apiSubnetName
        properties: {
          addressPrefix: '10.0.2.0/24'
          delegations: [
            {
              name: 'delegation'
              properties: {
                serviceName: 'Microsoft.Web/serverFarms'
              }
            }
          ]
          serviceEndpoints: [
            { service: 'Microsoft.KeyVault' }
            { service: 'Microsoft.Sql' }
          ]
        }
      }
    ]
  }
}

output funcSubnetId string = vnet.properties.subnets[0].id
output apiSubnetId string = vnet.properties.subnets[1].id

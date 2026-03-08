param appName string
param environment string
param location string

@description('Storage SKU. LRS for dev/test, GRS for prod.')
param storageSkuName string = 'Standard_LRS'

var storageAccountName = toLower('st${replace(appName, '-', '')}${environment}')

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  tags: {
    application: appName
    environment: environment
  }
  sku: {
    name: storageSkuName
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }
}

output storageAccountName string = storageAccount.name

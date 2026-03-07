targetScope = 'subscription'

@description('Base name used for all resources.')
param appName string = 'JG-WeatherHistory'

@description('Environment short code.')
@allowed(['d', 't', 'p'])
param environment string

@description('Azure region for all resources.')
param location string = 'swedencentral'

@secure()
@description('SQL Server administrator login name. Injected by pipeline at deploy time.')
param sqlAdminLogin string = ''

@secure()
@description('SQL Server administrator login password. Injected by pipeline at deploy time.')
param sqlAdminPassword string = ''

@description('SQL Database SKU name.')
param sqlSkuName string = 'Basic'

@description('SQL Database SKU tier.')
param sqlSkuTier string = 'Basic'

@description('IP address of the deploy agent allowed through SQL firewall. Leave empty to skip.')
param agentIpAddress string = ''

var resourceGroupName = 'rg-${appName}-${environment}'

resource rg 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: resourceGroupName
  location: location
  tags: {
    application: appName
    environment: environment
  }
}

module sql 'Modules/sql.bicep' = {
  name: 'sql-deploy'
  scope: rg
  params: {
    appName: appName
    environment: environment
    location: location
    sqlAdminLogin: sqlAdminLogin
    sqlAdminPassword: sqlAdminPassword
    sqlSkuName: sqlSkuName
    sqlSkuTier: sqlSkuTier
    agentIpAddress: agentIpAddress
  }
}

module storage 'Modules/storage.bicep' = {
  name: 'storage-deploy'
  scope: rg
  params: {
    appName: appName
    environment: environment
    location: location
  }
}

module asp 'Modules/appserviceplan.bicep' = {
  name: 'asp-deploy'
  scope: rg
  params: {
    appName: appName
    environment: environment
    location: location
  }
}

module monitoring 'Modules/monitoring.bicep' = {
  name: 'monitoring-deploy'
  scope: rg
  params: {
    appName: appName
    environment: environment
    location: location
  }
}

module func 'Modules/functionapp.bicep' = {
  name: 'functionapp-deploy'
  scope: rg
  params: {
    appName: appName
    environment: environment
    location: location
    storageAccountName: storage.outputs.storageAccountName
    appServicePlanId: asp.outputs.appServicePlanId
    appInsightsConnectionString: monitoring.outputs.appInsightsConnectionString
  }
}

module kv 'Modules/keyvault.bicep' = {
  name: 'keyvault-deploy'
  scope: rg
  params: {
    appName: appName
    environment: environment
    location: location
    functionAppPrincipalId: func.outputs.functionAppPrincipalId
  }
}

output resourceGroupName string = rg.name
output sqlServerName string = sql.outputs.sqlServerName
output sqlServerFqdn string = sql.outputs.sqlServerFqdn
output sqlDatabaseName string = sql.outputs.sqlDatabaseName
output keyVaultName string = kv.outputs.keyVaultName
output keyVaultUri string = kv.outputs.keyVaultUri
output functionAppName string = func.outputs.functionAppName
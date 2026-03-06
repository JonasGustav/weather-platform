targetScope = 'subscription'

@description('Base name used for all resources.')
param appName string = 'JG-WeatherHistory'

@description('Environment short code.')
@allowed(['d', 't', 'p'])
param environment string

@description('Azure region for all resources.')
param location string = 'swedencentral'

@secure()
@description('SQL Server administrator login name.')
param sqlAdminLogin string

@secure()
@description('SQL Server administrator login password.')
param sqlAdminPassword string

@description('SQL Database SKU name.')
param sqlSkuName string = 'Basic'

@description('SQL Database SKU tier.')
param sqlSkuTier string = 'Basic'

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
  }
}

output resourceGroupName string = rg.name
output sqlServerName string = sql.outputs.sqlServerName
output sqlServerFqdn string = sql.outputs.sqlServerFqdn
output sqlDatabaseName string = sql.outputs.sqlDatabaseName
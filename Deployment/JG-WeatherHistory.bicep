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

@description('Azure AD Tenant ID for API authentication.')
param aadTenantId string = ''

@description('Azure AD Client ID (App Registration) for API authentication.')
param aadClientId string = ''

@description('Email address for alert notifications.')
param alertEmail string = ''

@description('Whether alert rules are enabled. Should only be true in production.')
param alertsEnabled bool = false

var resourceGroupName = 'rg-${appName}-${environment}'
var keyVaultName = toLower('kv-${appName}-${environment}')
var keyVaultUri = 'https://${keyVaultName}${az.environment().suffixes.keyvaultDns}/'

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
    funcSubnetId: vnet.outputs.funcSubnetId
    apiSubnetId: vnet.outputs.apiSubnetId
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

module vnet 'Modules/vnet.bicep' = {
  name: 'vnet-deploy'
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
    keyVaultUri: keyVaultUri
    agentIpAddress: agentIpAddress
    subnetId: vnet.outputs.funcSubnetId
  }
}

module api 'Modules/webapp.bicep' = {
  name: 'webapp-deploy'
  scope: rg
  params: {
    appName: appName
    environment: environment
    location: location
    appServicePlanId: asp.outputs.appServicePlanId
    appInsightsConnectionString: monitoring.outputs.appInsightsConnectionString
    keyVaultUri: keyVaultUri
    agentIpAddress: agentIpAddress
    subnetId: vnet.outputs.apiSubnetId
    aadTenantId: aadTenantId
    aadClientId: aadClientId
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
    apiPrincipalId: api.outputs.webAppPrincipalId
    funcSubnetId: vnet.outputs.funcSubnetId
    apiSubnetId: vnet.outputs.apiSubnetId
    sqlServerFqdn: sql.outputs.sqlServerFqdn
    sqlDatabaseName: sql.outputs.sqlDatabaseName
    sqlAdminLogin: sqlAdminLogin
    sqlAdminPassword: sqlAdminPassword
  }
}

module alerts 'Modules/alerts.bicep' = {
  name: 'alerts-deploy'
  scope: rg
  params: {
    appName: appName
    environment: environment
    location: location
    logAnalyticsWorkspaceId: monitoring.outputs.logAnalyticsWorkspaceId
    functionAppName: func.outputs.functionAppName
    apiAppName: api.outputs.webAppName
    alertEmail: alertEmail
    alertsEnabled: alertsEnabled
  }
}

output resourceGroupName string = rg.name
output sqlServerName string = sql.outputs.sqlServerName
output sqlServerFqdn string = sql.outputs.sqlServerFqdn
output sqlDatabaseName string = sql.outputs.sqlDatabaseName
output keyVaultName string = kv.outputs.keyVaultName
output keyVaultUri string = kv.outputs.keyVaultUri
output functionAppName string = func.outputs.functionAppName
output apiAppName string = api.outputs.webAppName

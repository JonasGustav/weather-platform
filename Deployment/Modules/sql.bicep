param appName string
param environment string
param location string

@secure()
param sqlAdminLogin string

@secure()
param sqlAdminPassword string

param sqlSkuName string = 'Basic'
param sqlSkuTier string = 'Basic'

@description('IP address of the deploy agent allowed through SQL firewall. Leave empty to skip.')
param agentIpAddress string = ''

var sqlServerName = toLower('sql-${appName}-${environment}')
var sqlDatabaseName = 'sqldb-${appName}-${environment}'

resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: location
  tags: {
    application: appName
    environment: environment
  }
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: sqlSkuName
    tier: sqlSkuTier
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
  }
}

resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource allowAgentIp 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = if (!empty(agentIpAddress)) {
  parent: sqlServer
  name: 'AllowDeployAgent'
  properties: {
    startIpAddress: agentIpAddress
    endIpAddress: agentIpAddress
  }
}

output sqlServerName string = sqlServer.name
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDatabase.name

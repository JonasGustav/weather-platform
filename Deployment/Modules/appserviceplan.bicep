param appName string
param environment string
param location string

@description('SKU name. B1 = Basic')
param skuName string = 'B1'
param skuTier string = 'Basic'

var appServicePlanName = 'asp-${appName}-${environment}'

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  tags: {
    application: appName
    environment: environment
  }
  sku: {
    name: skuName
    tier: skuTier
  }
  properties: {}
}

output appServicePlanId string = appServicePlan.id
output appServicePlanName string = appServicePlan.name

using '../JG-WeatherHistory.bicep'

param environment = 'p'
param location = 'swedencentral'
param sqlSkuName = 'Basic'
param sqlSkuTier = 'Basic'
param aspSkuName = 'B1'
param aspSkuTier = 'Basic'
param storageSkuName = 'Standard_GRS'
param logRetentionDays = 30
param alertsEnabled = true

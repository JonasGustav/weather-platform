using '../JG-WeatherHistory.bicep'

param environment = 'd'
param location = 'swedencentral'
param sqlSkuName = 'Basic'
param sqlSkuTier = 'Basic'
param aspSkuName = 'B1'
param aspSkuTier = 'Basic'
param storageSkuName = 'Standard_LRS'
param logRetentionDays = 30
param alertsEnabled = false

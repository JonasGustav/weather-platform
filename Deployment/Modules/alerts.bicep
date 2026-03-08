param appName string
param environment string
param location string
param logAnalyticsWorkspaceId string
param functionAppName string
param apiAppName string
param alertEmail string
param alertsEnabled bool = false

var actionGroupName = 'ag-${appName}-${environment}'

resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: actionGroupName
  location: 'global'
  tags: {
    application: appName
    environment: environment
  }
  properties: {
    groupShortName: 'WeatherAlert'
    enabled: alertsEnabled
    emailReceivers: [
      {
        name: 'Owner'
        emailAddress: alertEmail
        useCommonAlertSchema: true
      }
    ]
  }
}

resource funcWarningAlert 'Microsoft.Insights/scheduledQueryRules@2023-03-15-preview' = {
  name: 'alert-func-warning-${environment}'
  location: location
  tags: {
    application: appName
    environment: environment
  }
  properties: {
    displayName: 'Function App - Warning or higher log'
    severity: 2
    enabled: alertsEnabled
    evaluationFrequency: 'PT1H'
    windowSize: 'PT1H'
    scopes: [logAnalyticsWorkspaceId]
    criteria: {
      allOf: [
        {
          query: 'AppTraces\n| where AppRoleName == \'${functionAppName}\'\n| where SeverityLevel >= 2'
          timeAggregation: 'Count'
          operator: 'GreaterThan'
          threshold: 0
          failingPeriods: {
            numberOfEvaluationPeriods: 1
            minFailingPeriodsToAlert: 1
          }
        }
      ]
    }
    actions: {
      actionGroups: [actionGroup.id]
    }
  }
}

resource funcSyncMissedAlert 'Microsoft.Insights/scheduledQueryRules@2023-03-15-preview' = {
  name: 'alert-func-sync-missed-${environment}'
  location: location
  tags: {
    application: appName
    environment: environment
  }
  properties: {
    displayName: 'Function App - SyncWeather not run in 1h'
    severity: 1
    enabled: alertsEnabled
    evaluationFrequency: 'PT1H'
    windowSize: 'PT1H10M'
    scopes: [logAnalyticsWorkspaceId]
    criteria: {
      allOf: [
        {
          query: 'AppRequests\n| where AppRoleName == \'${functionAppName}\'\n| where OperationName == \'SyncWeather\'\n| where Success == true'
          timeAggregation: 'Count'
          operator: 'LessThan'
          threshold: 1
          failingPeriods: {
            numberOfEvaluationPeriods: 1
            minFailingPeriodsToAlert: 1
          }
        }
      ]
    }
    actions: {
      actionGroups: [actionGroup.id]
    }
  }
}

resource apiWarningAlert 'Microsoft.Insights/scheduledQueryRules@2023-03-15-preview' = {
  name: 'alert-api-warning-${environment}'
  location: location
  tags: {
    application: appName
    environment: environment
  }
  properties: {
    displayName: 'API - Warning or higher log'
    severity: 2
    enabled: alertsEnabled
    evaluationFrequency: 'PT1H'
    windowSize: 'PT1H'
    scopes: [logAnalyticsWorkspaceId]
    criteria: {
      allOf: [
        {
          query: 'AppTraces\n| where AppRoleName == \'${apiAppName}\'\n| where SeverityLevel >= 2\n| where Message !contains \'IDW\'\n| where Message !contains \'Bearer\'\n| where Message !contains \'Authorization failed\'\n| where Message !contains \'401\''
          timeAggregation: 'Count'
          operator: 'GreaterThan'
          threshold: 0
          failingPeriods: {
            numberOfEvaluationPeriods: 1
            minFailingPeriodsToAlert: 1
          }
        }
      ]
    }
    actions: {
      actionGroups: [actionGroup.id]
    }
  }
}

param location string = resourceGroup().location
param environment string = 'dev'
param functionAppName string = 'bd-api-${uniqueString(resourceGroup().id)}'
param signalrName string = 'bd-signalr-${uniqueString(resourceGroup().id)}'
param staticSiteName string = 'bd-web-${uniqueString(resourceGroup().id)}'
param sku string = 'Y1' // Consumption for Function App

// Storage for Function App
resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: toLower(replace(functionAppName, '-', ''))
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

// App Service plan (Consumption) for Function App
resource plan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: functionAppName
  location: location
  sku: {
    name: sku
    tier: 'Dynamic'
  }
  kind: 'functionapp'
}

resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      appSettings: [
        { name: 'FUNCTIONS_WORKER_RUNTIME'; value: 'dotnet-isolated' }
        { name: 'AzureWebJobsStorage'; value: storage.properties.primaryEndpoints.blob }
        { name: 'WEBSITE_RUN_FROM_PACKAGE'; value: '1' }
        { name: 'SignalR__Mode'; value: 'Azure' }
        { name: 'SignalR__HubUrl'; value: 'https://' + signalrName + '.service.signalr.net' }
        { name: 'Jwt__Issuer'; value: 'bytedefence-' + environment }
        { name: 'Jwt__Audience'; value: 'bytedefence-clients' }
      ]
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource signalr 'Microsoft.SignalRService/signalR@2023-02-01' = {
  name: signalrName
  location: location
  sku: {
    name: 'Free_F1'
    capacity: 1
  }
  properties: {
    features: [
      {
        flag: 'ServiceMode'
        value: 'Default'
      }
    ]
    cors: {
      allowedOrigins: [ 'http://localhost:8080' ]
    }
  }
}

resource staticSite 'Microsoft.Web/staticSites@2022-03-01' = {
  name: staticSiteName
  location: location
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    allowConfigFileUpdates: true
    buildProperties: {
      appLocation: '/'
      apiLocation: ''
      appArtifactLocation: 'wwwroot'
    }
  }
}

output functionHost string = 'https://' + functionAppName + '.azurewebsites.net'
output signalrHost string = 'https://' + signalrName + '.service.signalr.net'
output staticSiteHost string = 'https://' + staticSiteName + '.z01.azurefd.net'

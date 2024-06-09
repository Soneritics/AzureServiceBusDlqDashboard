# Azure Servicebus Dlq Dashboard
_You will have to make the dashboard yourself :-)_

## Function overview
  - [GET] /api/GetStatuses
  - [GET] /api/EmptyQueueDlqMessages
  - [GET] /api/EmptyTopicDlqMessages
  - [GET] /api/GetQueueDlqMessages
  - [GET] /api/GetTopicDlqMessages
  - [GET] /api/RetryQueueDlqMessages
  - [GET] /api/RetryTopicDlqMessages

## Configuration
For development purposes, you can create a local.settings.json that has the following contents:
```csharp
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true", // necessary for the durable function
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated", // we run a .NET 8 v4 isolated Azure Function App

        "UpdateSchedule": "0 */1 * * * *", // every minute for development purposes

        // Just an example list of service bus connection strings
        "Bus__Prd": "Endpoint=sb://bus-prd.servicebus.windows.net/;SharedAccessKeyName=KeyName;SharedAccessKey=yoursecret",
        "Bus__Acc": "Endpoint=sb://bus-acc.servicebus.windows.net/;SharedAccessKeyName=KeyName;SharedAccessKey=yoursecret",
        "Bus__Dev": "Endpoint=sb://bus-dev.servicebus.windows.net/;SharedAccessKeyName=KeyName;SharedAccessKey=yoursecret"
    }
}
```
Everything that starts with `Bus__` gets treated as a service bus connection string.

When deploying, make sure to set the `UpdateSchedule` to a more reasonable value. This also depends on the number of buses, queues, topics and subscriptions and how often you need to have the data refreshed.

## Deployment using Azure DevOps pipelines
When you want to deploy this app as-is, the YML pipeline can be configured as the script below.
This is just an example of how to use the Azure DevOps pipeline to deploy the Azure Function App.

This script deploys to three environments (dev, acc, prd) and uses a preconfigured service connection to authenticate with Azure.

Also don't forget to change your app name. The app name is the name of the function app in Azure.
```yaml
trigger:
- main

variables:
  gen.ProjectName: 'DlqDashboard'
  
  dev.appSettings: '-UpdateSchedule "0 */5 * * * *"
      -AzureWebJobs.UpdateStarter.Disabled "1"
      -Bus__Example-One "@Microsoft.KeyVault(SecretUri=https://xxxx-kv-dev.vault.azure.net/secrets/ServiceBusConnectionStringOne)"
      -Bus__Example-Two "@Microsoft.KeyVault(SecretUri=https://xxxx-kv-dev.vault.azure.net/secrets/ServiceBusConnectionStringTwo)"
      -Bus__Example-Six "@Microsoft.KeyVault(SecretUri=https://xxxx-kv-dev.vault.azure.net/secrets/ServiceBusConnectionStringSix)"'
  acc.appSettings: '-UpdateSchedule "0 */15 * * * *"
      -AzureWebJobs.UpdateStarter.Disabled "1"
      -Bus__Example-One "@Microsoft.KeyVault(SecretUri=https://xxxx-kv-acc.vault.azure.net/secrets/ServiceBusConnectionStringOne)"
      -Bus__Example-Two "@Microsoft.KeyVault(SecretUri=https://xxxx-kv-acc.vault.azure.net/secrets/ServiceBusConnectionStringTwo)"
      -Bus__Example-Six "@Microsoft.KeyVault(SecretUri=https://xxxx-kv-acc.vault.azure.net/secrets/ServiceBusConnectionStringSix)"'
  prd.appSettings: '-UpdateSchedule "0 */5 * * * *"
      -AzureWebJobs.UpdateStarter.Disabled "0"
      -Bus__Example-One "@Microsoft.KeyVault(SecretUri=https://xxxx-kv-prd.vault.azure.net/secrets/ServiceBusConnectionStringOne)"
      -Bus__Example-Two "@Microsoft.KeyVault(SecretUri=https://xxxx-kv-prd.vault.azure.net/secrets/ServiceBusConnectionStringTwo)"
      -Bus__Example-Six "@Microsoft.KeyVault(SecretUri=https://xxxx-kv-prd.vault.azure.net/secrets/ServiceBusConnectionStringSix)"'

stages:
# Stage 1: Build and run tests
- stage: Build
  displayName: 'Clone repo from Github, build and publish the result'
  jobs:
  - job: CloneBuildAndPublishArtefact
    displayName: 'Clone, build and publish'
    pool:
      vmImage: 'windows-latest'
    steps:
    - script: |
        git clone https://github.com/Soneritics/AzureServiceBusDlqDashboard.git
        cd AzureServiceBusDlqDashboard
      displayName: 'Clone repository from Github'

    - task: NuGetToolInstaller@1

    - task: NuGetCommand@2
      displayName: 'Restore Nuget'
      inputs:
        command: 'restore'
        restoreSolution: '**/*.sln'
    
    - task: DotNetCoreCLI@2
      displayName: 'Build Project'
      inputs:
        command: 'build'
        projects: '**/*.csproj'

    - task: DotNetCoreCLI@2
      displayName: 'Publish Function App Project'
      inputs:
        command: 'publish'
        publishWebProjects: false
        projects: '**/App/*.csproj'
        arguments: '--configuration $(BuildConfiguration) --output $(Build.ArtifactStagingDirectory)'
        zipAfterPublish: true
        modifyOutputPath: true

    - task: PublishBuildArtifacts@1
      displayName: 'Create Artifacts Drop'
      inputs:
        PathtoPublish: '$(Build.ArtifactStagingDirectory)'
        ArtifactName: 'drop'
        publishLocation: 'Container'

# Stage 2: Deploy to dev environment
- stage: DeployDev
  displayName: 'Deploy: Development'
  jobs:
  - deployment: 'Deploy'
    displayName: 'Deploy: Development'
    pool:
      vmImage: 'windows-latest'
    environment: 'Dev'
    strategy:
      runOnce:
        deploy:
            steps:
            - task: AzureFunctionApp@1
              displayName: Azure Function App Deploy
              inputs:
                azureSubscription: 'service-connection-dev'
                appType: functionApp
                appName: 'dlqdashboard-dev'
                package: '$(Pipeline.Workspace)\drop\App.zip'
                appSettings: '$(dev.appSettings)'

# Stage 3: Deploy to dev environment
- stage: DeployAcc
  displayName: 'Deploy: Acceptance'
  jobs:
  - deployment: 'Deploy'
    displayName: 'Deploy: Acceptance'
    pool:
      vmImage: 'windows-latest'
    environment: 'Acc'
    strategy:
      runOnce:
        deploy:
            steps:
            - task: AzureFunctionApp@1
              displayName: Azure Function App Deploy
              inputs:
                azureSubscription: 'service-connection-acc'
                appType: functionApp
                appName: 'dlqdashboard-acc'
                package: '$(Pipeline.Workspace)\drop\App.zip'
                appSettings: '$(acc.appSettings)'

# Stage 4: Deploy to dev environment
- stage: DeployPrd
  displayName: 'Deploy: Production'
  jobs:
  - deployment: 'Deploy'
    displayName: 'Deploy: Production'
    pool:
      vmImage: 'windows-latest'
    environment: 'Prd'
    strategy:
      runOnce:
        deploy:
            steps:
            - task: AzureFunctionApp@1
              displayName: Azure Function App Deploy
              inputs:
                azureSubscription: 'service-connection-prd'
                appType: functionApp
                appName: 'dlqdashboard-prd'
                package: '$(Pipeline.Workspace)\drop\App.zip'
                appSettings: '$(prd.appSettings)'
```

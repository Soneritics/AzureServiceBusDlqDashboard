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

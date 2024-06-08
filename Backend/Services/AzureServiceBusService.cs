using Azure.Messaging.ServiceBus.Administration;
using Domain;

namespace Backend.Services;

public class AzureServiceBusService : IAzureServiceBusService
{
    public async Task<IEnumerable<Queue>> GetQueuesAsync(string connectionString)
    {
        var client = new ServiceBusAdministrationClient(connectionString);
        var queues = new List<Queue>();
        await foreach (var queue in client.GetQueuesRuntimePropertiesAsync())
        {
            queues.Add(new Queue(queue.Name)
            {
                MessageCount = queue.ActiveMessageCount,
                DeadLetterMessageCount = queue.DeadLetterMessageCount
            });
        }

        return queues;
    }

    public async Task<IEnumerable<Topic>> GetTopicsAsync(string connectionString)
    {
        var client = new ServiceBusAdministrationClient(connectionString);
        var topics = new List<Topic>();
        await foreach (var topic in client.GetTopicsAsync())
        {
            var subscriptions = new List<TopicSubscription>();
            await foreach (var properties in client.GetSubscriptionsRuntimePropertiesAsync(topic.Name))
            {
                subscriptions.Add(new TopicSubscription(properties.SubscriptionName)
                {
                    MessageCount = properties.ActiveMessageCount,
                    DeadLetterMessageCount = properties.DeadLetterMessageCount
                });
            }
            
            topics.Add(new Topic(topic.Name)
            {
                Subscriptions = subscriptions
            });
        }

        return topics;
    }
}
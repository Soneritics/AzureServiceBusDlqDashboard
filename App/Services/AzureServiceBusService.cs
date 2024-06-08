using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Domain.Message;
using Domain.Status;
using ServiceBusMessage = Domain.Message.ServiceBusMessage;

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

    public async Task<IEnumerable<ServiceBusMessage>> GetQueueDlqMessagesAsync(
        string connectionString,
        string queueName,
        bool resubmitMessages = false,
        bool completeMessages = false)
    {
        await using var client = new ServiceBusClient(connectionString);
        var dlqReceiver = client.CreateReceiver($"{queueName}/$DeadLetterQueue");

        var dlqMessages = (await dlqReceiver.ReceiveMessagesAsync(100))?.ToList()
            ?? new List<ServiceBusReceivedMessage>();

        if (resubmitMessages || completeMessages)
        {
            foreach (var message in dlqMessages)
            {
                if (resubmitMessages)
                {
                    await client
                        .CreateSender(queueName)
                        .SendMessageAsync(new Azure.Messaging.ServiceBus.ServiceBusMessage(message));
                }

                if (completeMessages)
                {
                    await dlqReceiver.CompleteMessageAsync(message);
                }
            }
        }

        return dlqMessages.Select(m => m.Map());
    }

    public async Task<IEnumerable<ServiceBusMessage>> GetTopicDlqMessagesAsync(
        string connectionString,
        string topicName,
        string subscriptionName,
        bool resubmitMessages = false,
        bool completeMessages = false)
    {
        await using var client = new ServiceBusClient(connectionString);
        var dlqReceiver = client.CreateReceiver($"{topicName}/Subscriptions/{subscriptionName}/$DeadLetterQueue");

        var dlqMessages = (await dlqReceiver.ReceiveMessagesAsync(100))?.ToList()
            ?? new List<ServiceBusReceivedMessage>();

        if (resubmitMessages || completeMessages)
        {
            foreach (var message in dlqMessages)
            {
                if (resubmitMessages)
                {
                    await client
                        .CreateSender(topicName)
                        .SendMessageAsync(new Azure.Messaging.ServiceBus.ServiceBusMessage(message));
                }

                if (completeMessages)
                {
                    await dlqReceiver.CompleteMessageAsync(message);
                }
            }
        }

        return dlqMessages.Select(m => m.Map());
    }
}
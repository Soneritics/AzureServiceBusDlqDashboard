using Domain.Status;
using ServiceBusMessage = Domain.Message.ServiceBusMessage;

namespace Backend.Services;

public interface IAzureServiceBusService
{
    Task<IEnumerable<Queue>> GetQueuesAsync(string connectionString);
    Task<IEnumerable<Topic>> GetTopicsAsync(string connectionString);

    Task<IEnumerable<ServiceBusMessage>> GetQueueDlqMessagesAsync(
        string connectionString,
        string queueName,
        bool resubmitMessages = false,
        bool completeMessages = false);

    Task<IEnumerable<ServiceBusMessage>> GetTopicDlqMessagesAsync(
        string connectionString,
        string topicName,
        string subscriptionName,
        bool resubmitMessages = false,
        bool completeMessages = false);
}
using Domain;

namespace Backend.Services;

public interface IAzureServiceBusService
{
    Task<IEnumerable<Queue>> GetQueuesAsync(string connectionString);
    Task<IEnumerable<Topic>> GetTopicsAsync(string connectionString);
}
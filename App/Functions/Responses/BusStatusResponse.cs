using Domain.Status;

namespace Backend.Functions.Responses;

public class BusStatusResponse(string id, IEnumerable<Queue> queues, IEnumerable<Topic> topics)
{
    public IEnumerable<Topic> Topics { get; set; } = topics;

    public IEnumerable<Queue> Queues { get; set; } = queues;

    public string Id { get; set; } = id;
}
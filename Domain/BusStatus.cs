namespace Domain;

public class BusStatus(string id, string connectionString)
{
    public string Id { get; set; } = id;
    public string ConnectionString { get; set; } = connectionString;
    public IEnumerable<Queue> Queues { get; set; } = [];
    public IEnumerable<Topic> Topics { get; set; } = [];
}
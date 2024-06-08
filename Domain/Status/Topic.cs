namespace Domain.Status;

public class Topic(string name)
{
    public string Name { get; set; } = name;
    public IEnumerable<TopicSubscription> Subscriptions { get; set; } = [];
}
namespace Domain;

public class Queue(string name)
{
    public string Name { get; set; } = name;
    public long MessageCount { get; set; }
    public long DeadLetterMessageCount { get; set; }
}
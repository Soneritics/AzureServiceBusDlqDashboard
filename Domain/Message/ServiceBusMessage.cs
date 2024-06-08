namespace Domain.Message;

public class ServiceBusMessage
{
    public string? MessageId { get; set; }
    public string? JsonBody { get; set; }
    public string? DeadLetterReason { get; set; }
    public string? DeadLetterErrorDescription { get; set; }
    public DateTime EnqueuedTime { get; set; }
}
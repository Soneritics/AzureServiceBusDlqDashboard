using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Domain.Message;

public static class Mapper
{
    public static ServiceBusMessage Map(this ServiceBusReceivedMessage message)
    {
        var stringBody = message.Body.ToString();
        var content = JsonConvert.DeserializeObject<JObject>(stringBody)?["Content"];
        if (content != null)
        {
            stringBody = JsonConvert.SerializeObject(content, Formatting.Indented);
        }
        
        return new ServiceBusMessage()
        {
            MessageId = message.MessageId,
            DeadLetterReason = message.DeadLetterReason,
            DeadLetterErrorDescription = message.DeadLetterErrorDescription,
            EnqueuedTime = message.EnqueuedTime.DateTime,
            JsonBody = stringBody
        };
    }
}
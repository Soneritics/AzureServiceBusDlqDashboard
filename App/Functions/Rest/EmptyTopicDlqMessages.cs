using System.Net;
using Backend.Repository;
using Backend.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Backend.Functions.Rest;

public class EmptyTopicDlqMessages(IAzureServiceBusService serviceBusService)
{
    [Function(nameof(EmptyTopicDlqMessages))]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get")]
        HttpRequestData req,
        string busName,
        string topicName,
        string subscriptionName,
        FunctionContext executionContext)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);

        if (string.IsNullOrEmpty(busName) || string.IsNullOrEmpty(topicName) || string.IsNullOrEmpty(subscriptionName))
        {
            await response.WriteAsJsonAsync(
                new
                {
                    Error = "BusName and TopicName and SubscriptionName must be provided"
                },
                HttpStatusCode.BadRequest);
        }
        else if (!BusStatusRepository.BusStatuses.Any(s => s.Id.Equals(busName)))
        {
            await response.WriteAsJsonAsync(
                new
                {
                    Error = "BusName not found"
                },
                HttpStatusCode.BadRequest);
        }
        else
        {
            var bus = BusStatusRepository.BusStatuses.Find(s => s.Id.Equals(busName));
            
            await response.WriteAsJsonAsync(
                await serviceBusService.GetTopicDlqMessagesAsync(
                    bus!.ConnectionString,
                    topicName,
                    subscriptionName,
                    false,
                    true));
            
            bus
                .Topics
                .First(t => t.Name.Equals(topicName))
                .Subscriptions
                .First(s => s.Name.Equals(subscriptionName))
                .DeadLetterMessageCount = 0;
        }
        
        return response;
    }
}
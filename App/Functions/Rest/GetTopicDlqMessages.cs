using System.Net;
using Backend.Repository;
using Backend.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Backend.Functions.Rest;

public class GetTopicDlqMessages(IAzureServiceBusService serviceBusService)
{
    [Function(nameof(GetTopicDlqMessages))]
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
            await response.WriteAsJsonAsync(
                await serviceBusService.GetTopicDlqMessagesAsync(
                    BusStatusRepository.BusStatuses.Find(s => s.Id.Equals(busName))!.ConnectionString,
                    topicName,
                    subscriptionName));
        }
        
        return response;
    }
}
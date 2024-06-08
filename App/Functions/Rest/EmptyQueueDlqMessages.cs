using System.Net;
using Backend.Repository;
using Backend.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Backend.Functions.Rest;

public class EmptyQueueDlqMessages(IAzureServiceBusService serviceBusService)
{
    [Function(nameof(EmptyQueueDlqMessages))]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get")]
        HttpRequestData req,
        string busName,
        string queueName,
        FunctionContext executionContext)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);

        if (string.IsNullOrEmpty(busName) || string.IsNullOrEmpty(queueName))
        {
            await response.WriteAsJsonAsync(
                new
                {
                    Error = "BusName and QueueName must be provided"
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
                await serviceBusService.GetQueueDlqMessagesAsync(
                    bus!.ConnectionString,
                    queueName,
                    false,
                    true));
            
            bus.Queues.First(q => q.Name.Equals(queueName)).DeadLetterMessageCount = 0;
        }
        
        return response;
    }
}
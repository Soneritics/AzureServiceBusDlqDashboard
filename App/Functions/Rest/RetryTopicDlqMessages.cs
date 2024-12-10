using System.Net;
using Backend.Functions.Responses;
using Backend.Repository;
using Backend.Services;
using Domain.Message;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;

namespace Backend.Functions.Rest;

public class RetryTopicDlqMessages(IAzureServiceBusService serviceBusService)
{
    [OpenApiOperation(operationId: nameof(RetryTopicDlqMessages))]
    [OpenApiParameter(nameof(busName), In = ParameterLocation.Query, Required = true, Type = typeof(string))]
    [OpenApiParameter(nameof(topicName), In = ParameterLocation.Query, Required = true, Type = typeof(string))]
    [OpenApiParameter(nameof(subscriptionName), In = ParameterLocation.Query, Required = true, Type = typeof(string))]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ServiceBusMessage>))]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(ErrorResponse))]
    [Function(nameof(RetryTopicDlqMessages))]
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
                new ErrorResponse("BusName and TopicName and SubscriptionName must be provided"),
                HttpStatusCode.BadRequest);
        }
        else if (!BusStatusRepository.BusStatuses.Any(s => s.Id.Equals(busName)))
        {
            await response.WriteAsJsonAsync(
                new ErrorResponse("BusName not found"),
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
                    true,
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
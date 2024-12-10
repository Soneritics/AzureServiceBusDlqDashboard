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

public class GetQueueDlqMessages(IAzureServiceBusService serviceBusService)
{
    [OpenApiOperation(operationId: nameof(GetQueueDlqMessages))]
    [OpenApiParameter(nameof(busName), In = ParameterLocation.Query, Required = true, Type = typeof(string))]
    [OpenApiParameter(nameof(queueName), In = ParameterLocation.Query, Required = true, Type = typeof(string))]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ServiceBusMessage>))]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(ErrorResponse))]
    [Function(nameof(GetQueueDlqMessages))]
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
                new ErrorResponse("BusName and QueueName must be provided"),
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
            await response.WriteAsJsonAsync(
                await serviceBusService.GetQueueDlqMessagesAsync(
                    BusStatusRepository.BusStatuses.Find(s => s.Id.Equals(busName))!.ConnectionString,
                    queueName));
        }
        
        return response;
    }
}
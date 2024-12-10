using System.Net;
using Backend.Functions.Responses;
using Backend.Repository;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Backend.Functions.Rest;

public class GetStatuses
{
    [OpenApiOperation(operationId: nameof(GetStatuses))]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<BusStatusResponse>))]
    [Function(nameof(GetStatuses))]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(BusStatusRepository
            .BusStatuses
            .Select(s => new BusStatusResponse(s.Id, s.Queues, s.Topics)));
        
        return response;
    }
}
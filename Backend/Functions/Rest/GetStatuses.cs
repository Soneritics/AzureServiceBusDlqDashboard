using System.Net;
using Backend.Repository;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Backend.Functions.Rest;

public class GetStatuses
{
    [Function(nameof(GetStatuses))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(BusStatusRepository
            .BusStatuses
            .Select(s => new
            {
                s.Id,
                s.Queues,
                s.Topics
            }));
        
        return response;
    }
}
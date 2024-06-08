using Backend.Repository;
using Backend.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace Backend.Functions.Durable;

public class UpdateProcessor(IAzureServiceBusService serviceBusService)
{
    [Function(nameof(UpdateProcessor))]
    public async Task RunAsync([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var processList = BusStatusRepository.BusStatuses.Select(s => s.Id).ToList();
        foreach (var busId in processList)
        {
            await context.CallActivityAsync(nameof(UpdateBusStatus), busId);
        }
    }

    [Function(nameof(UpdateBusStatus))]
    public async Task UpdateBusStatus([ActivityTrigger] string busId, FunctionContext executionContext)
    {
        var bus = BusStatusRepository.BusStatuses.Find(s => s.Id == busId);
        if (bus != null)
        {
            bus.Queues = await serviceBusService.GetQueuesAsync(bus.ConnectionString);
            bus.Topics = await serviceBusService.GetTopicsAsync(bus.ConnectionString);
        }
    }
}
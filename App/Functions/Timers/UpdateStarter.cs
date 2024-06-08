using Backend.Functions.Durable;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;

namespace Backend.Functions.Timers;

public class UpdateStarter
{
    [Function(nameof(UpdateStarter))]
    public async Task RunAsync(
        [TimerTrigger("%UpdateSchedule%")] TimerInfo myTimer,
        [DurableClient] DurableTaskClient starter)
    {
        await starter.ScheduleNewOrchestrationInstanceAsync(nameof(UpdateProcessor));
    }
}
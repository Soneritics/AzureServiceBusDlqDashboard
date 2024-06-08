using Backend.Repository;
using Backend.Services;
using Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights()
            .AddLogging(l =>
            {
                l.AddConsole();
                l.AddFilter(level => level >= LogLevel.Information);
            })
            .AddSingleton<IAzureServiceBusService, AzureServiceBusService>();
    })
    .Build();

BusStatusRepository.BusStatuses = Environment
    .GetEnvironmentVariables()
    .Keys
    .Cast<string>()
    .Where(k => k.StartsWith("Bus__"))
    .Select(s => new BusStatus(
        s["Bus__".Length..],
        Environment.GetEnvironmentVariable(s)!))
    .ToList();

await host.RunAsync();

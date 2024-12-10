using System.Text.Json;
using System.Text.Json.Serialization;
using Backend.Repository;
using Backend.Services;
using Domain.Status;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker =>
    {
        worker.Services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
    })
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
            .AddSingleton<IAzureServiceBusService, AzureServiceBusService>()
            .AddSingleton<IOpenApiConfigurationOptions>(_ =>
            {
                var options = new OpenApiConfigurationOptions()
                {
                    Info = new OpenApiInfo
                    {
                        Version = "1.0.0",
                        Title = "Bus Status API",
                        Description = "Azure Servicebus Dlq Dashboard API documentation",
                        Contact = new OpenApiContact
                        {
                            Name = "Jordi Jolink",
                            Email = "sbdashboard@jordijolink.nl",
                            Url = new Uri("https://github.com/Soneritics/AzureServiceBusDlqDashboard"),
                        },
                        License = new OpenApiLicense
                        {
                            Name = "MIT",
                            Url = new Uri("http://opensource.org/licenses/MIT"),
                        }
                    },
                    Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
                    OpenApiVersion = OpenApiVersionType.V2,
                    IncludeRequestingHostName = true,
                    ForceHttps = false,
                    ForceHttp = false,
                };

                return options;
            });
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

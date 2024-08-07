using Microsoft.Extensions.Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        // Dependency injection for EmailClient
        services.AddAzureClients(clientBuilder =>
        {
            var connectionString = hostContext.Configuration.GetConnectionString("COMMUNICATION_SERVICES_CONNECTION_STRING");
            clientBuilder.AddEmailClient(connectionString);
        });
    })
    .Build();

host.Run();



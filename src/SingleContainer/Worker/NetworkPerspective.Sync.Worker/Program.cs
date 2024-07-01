using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NetworkPerspective.Sync.Connector.Application;
using NetworkPerspective.Sync.Contract.V1.Impl;

namespace NetworkPerspective.Sync.Worker;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services
            .AddConnectorApplication()
            .AddOrchestratorClient(builder.Configuration.GetSection("Infrastructure:Orchestrator"));

        builder.Services.AddHostedService<ConnectionHost>();

        var host = builder.Build();
        host.Run();
    }
}
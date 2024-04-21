using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NetworkPerspective.Sync.Connector.Application;

namespace NetworkPerspective.Sync.Connector;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddConnectorApplication();
        builder.Services.AddSingleton<HubClient>();
        builder.Services.AddHostedService<Worker>();

        var host = builder.Build();
        host.Run();
    }
}
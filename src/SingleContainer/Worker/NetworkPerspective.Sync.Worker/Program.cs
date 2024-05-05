using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NetworkPerspective.Sync.Connector.Application;

namespace NetworkPerspective.Sync.Worker;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddConnectorApplication();
        builder.Services.AddSingleton<HubClient>();
        builder.Services.AddHostedService<ConnectionHost>();

        var host = builder.Build();
        host.Run();
    }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Contract.V1.Impl;
using NetworkPerspective.Sync.Infrastructure.Core;
using NetworkPerspective.Sync.Infrastructure.Slack;
using NetworkPerspective.Sync.Infrastructure.Slack.Services;
using NetworkPerspective.Sync.Worker.Application;

namespace NetworkPerspective.Sync.Worker;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        var healthChecksBuilder = builder.Services
            .AddHealthChecks();

        builder.Services
            .AddConnectorApplication(builder.Configuration.GetSection("App"))
            .AddNetworkPerspectiveCore(builder.Configuration.GetSection("Infrastructure:Core"), healthChecksBuilder)
            .AddVault(builder.Configuration.GetSection("Infrastructure:Vaults"), healthChecksBuilder)
            .AddSlack(builder.Configuration.GetSection("Infrastructure:DataSources:Slack"))
            .AddOrchestratorClient(builder.Configuration.GetSection("Infrastructure:Orchestrator"));

        builder.Services.RemoveAll(typeof(IAuthTester));
        builder.Services.RemoveAll(typeof(ISlackAuthService));
        builder.Services.RemoveAll(typeof(ISecretRotator));

        builder.Services
            .AddScoped<IAuthTester, DummyAuthTester>();

        builder.Services.AddHostedService<ConnectionHost>();

        var host = builder.Build();
        host.Run();
    }
}
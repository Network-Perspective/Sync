using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Contract.V1.Impl;
using NetworkPerspective.Sync.Infrastructure.Core;
using NetworkPerspective.Sync.Infrastructure.Slack;
using NetworkPerspective.Sync.Infrastructure.Slack.Services;
using NetworkPerspective.Sync.Infrastructure.Google;
using NetworkPerspective.Sync.Infrastructure.Excel;
using NetworkPerspective.Sync.Infrastructure.Microsoft;
using NetworkPerspective.Sync.Worker.Application;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;


#if !DEBUG
#else
using NetworkPerspective.Sync.Infrastructure.Core.Stub;
#endif


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
            .AddGoogle(builder.Configuration.GetSection("Infrastructure:DataSources:Google"))
            .AddMicrosoft(builder.Configuration.GetSection("Infrastructure:DataSources:Microsoft"))
            .AddExcel(builder.Configuration.GetSection("Infrastructure:DataSources:Excel"))
            .AddOrchestratorClient(builder.Configuration.GetSection("Infrastructure:Orchestrator"));

        builder.Services.RemoveAll(typeof(IAuthTester));
        builder.Services.RemoveAll(typeof(ISlackAuthService));
        builder.Services.RemoveAll(typeof(IMicrosoftAuthService));
        builder.Services.RemoveAll(typeof(ISecretRotator));

        builder.Services
            .AddScoped<IAuthTester, DummyAuthTester>();

        builder.Services.AddHostedService<ConnectionHost>();

        builder.Services.AddApplicationInsightsTelemetryWorkerService();

#if !DEBUG
        builder.Services.RemoveHttpClientLogging();
#else
        builder.Services.AddNetworkPerspectiveCoreStub(builder.Configuration.GetSection("Infrastructure:Core"));
#endif


        var host = builder.Build();
        host.Run();
    }
}
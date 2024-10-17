using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NetworkPerspective.Sync.Contract.V1.Impl;
using NetworkPerspective.Sync.Infrastructure.Core;
using NetworkPerspective.Sync.Worker.Application;
using NetworkPerspective.Sync.Infrastructure.DataSources.Excel;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack;

using System;
using System.Threading;

using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

#if !DEBUG
#else
using NetworkPerspective.Sync.Infrastructure.Core.Stub;
#endif

namespace NetworkPerspective.Sync.Worker;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();
        try
        {
            // Configure services
            builder.ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;
                var healthChecksBuilder = services.AddHealthChecks();

                services
                    .AddConnectorApplication(configuration.GetSection("App"))
                    .AddNetworkPerspectiveCore(configuration.GetSection("Infrastructure:Core"), healthChecksBuilder)
                    .AddVault(configuration.GetSection("Infrastructure:Vaults"), healthChecksBuilder)
                    .AddSlack(configuration.GetSection("Infrastructure:DataSources:Slack"))
                    .AddGoogle(configuration.GetSection("Infrastructure:DataSources:Google"))
                    .AddMicrosoft(configuration.GetSection("Infrastructure:DataSources:Microsoft"))
                    .AddJira(configuration.GetSection("Infrastructure:DataSources:Jira"))
                    .AddExcel(configuration.GetSection("Infrastructure:DataSources:Excel"))
                    .AddOrchestratorClient(configuration.GetSection("Infrastructure:Orchestrator"));

                services.AddHostedService<ConnectionHost>();

                services.AddApplicationInsightsTelemetryWorkerService();

#if !DEBUG
            services.RemoveHttpClientLogging();
#else
                services.AddNetworkPerspectiveCoreStub(configuration.GetSection("Infrastructure:Core"));
#endif
            });
            
            var enableWHealthChecks = config.GetValue<bool>("App:EnableHealthChecks");
            if (enableWHealthChecks)
            {
                // Conditionally enable Kestrel and WebHost
                builder.ConfigureWebHostDefaults(webBuilder =>
                {
                    {
                        // Configure Kestrel
                        webBuilder.ConfigureKestrel(options =>
                        {
                            options.ListenAnyIP(7000);
                        });

                        // Configure the app to use routing and map health checks
                        webBuilder.Configure(app =>
                        {
                            app.UseRouting();
                            app.UseEndpoints(endpoints =>
                            {
                                endpoints.MapHealthChecks("/health");
                            });
                        });
                    }
                });
            }

            var host = builder.Build();
            
            host.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            var delay = config.GetValue<TimeSpan>("App:DelayBeforeExitOnException");
            Thread.Sleep(delay);
            throw;
        }
    }
}
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

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

using System.Collections.Generic;

using NetworkPerspective.Sync.Worker.Application.Services;


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

        try
        {
            var connectorTypes = new List<ConnectorType>{
                new() { Name = "Slack",     DataSourceId = "SlackId",       DataSourceFacadeFullName = "NetworkPerspective.Sync.Infrastructure.DataSources.Slack.SlackFacade"},
                new() { Name = "Google",    DataSourceId = "GSuiteId",      DataSourceFacadeFullName = "NetworkPerspective.Sync.Infrastructure.DataSources.Google.GoogleFacade"},
                new() { Name = "Excel",     DataSourceId = "ExcelId",       DataSourceFacadeFullName = "NetworkPerspective.Sync.Infrastructure.DataSources.Excel.ExcelFacade"},
                new() { Name = "Office365", DataSourceId = "Office365Id",   DataSourceFacadeFullName = "NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.MicrosoftFacade"},
                new() { Name = "Jira",      DataSourceId = "JiraId",        DataSourceFacadeFullName = "NetworkPerspective.Sync.Infrastructure.DataSources.Jira.JiraFacade"},
            };

            var healthChecksBuilder = builder.Services
                .AddHealthChecks();

            builder.Services
                .AddWorkerApplication(builder.Configuration.GetSection("App"), connectorTypes)
                .AddNetworkPerspectiveCore(builder.Configuration.GetSection("Infrastructure:Core"), healthChecksBuilder)
                .AddVault(builder.Configuration.GetSection("Infrastructure:Vaults"), healthChecksBuilder)
                .AddSlack(builder.Configuration.GetSection("Infrastructure:DataSources:Slack"))
                .AddGoogle(builder.Configuration.GetSection("Infrastructure:DataSources:Google"))
                .AddMicrosoft(builder.Configuration.GetSection("Infrastructure:DataSources:Microsoft"))
                .AddJira(builder.Configuration.GetSection("Infrastructure:DataSources:Jira"))
                .AddExcel(builder.Configuration.GetSection("Infrastructure:DataSources:Excel"))
                .AddOrchestratorClient(builder.Configuration.GetSection("Infrastructure:Orchestrator"));

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
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            var delay = builder.Configuration.GetValue<TimeSpan>("App:DelayBeforeExitOnException");
            Thread.Sleep(delay);
            throw;
        }
    }
}
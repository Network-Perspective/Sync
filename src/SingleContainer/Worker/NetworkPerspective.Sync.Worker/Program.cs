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

using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Worker.Configs;

using NetworkPerspective.Sync.Worker.HostedServices;

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
            var slackConnector = new ConnectorType { Name = "Slack", DataSourceId = "SlackId" };
            var googleConnector = new ConnectorType { Name = "Google", DataSourceId = "GSuiteId" };
            var excelConnector = new ConnectorType { Name = "Excel", DataSourceId = "ExcelId" };
            var office365Connector = new ConnectorType { Name = "Office365", DataSourceId = "Office365Id" };
            var jiraConnector = new ConnectorType { Name = "Jira", DataSourceId = "JiraId" };

            var connectorTypes = new List<ConnectorType> {
                slackConnector,
                googleConnector,
                excelConnector,
                office365Connector,
                jiraConnector,
            };

            var healthChecksBuilder = builder.Services
                .AddHealthChecks();

            builder.Services
                .AddSingleton<IValidateOptions<WorkerConfig>, WorkerConfig.Validator>()
                .AddOptions<WorkerConfig>()
                .Bind(builder.Configuration)
                .ValidateOnStart();

            builder.Services
                .AddWorkerApplication(builder.Configuration.GetSection("App"), connectorTypes)
                .AddNetworkPerspectiveCore(builder.Configuration.GetSection("Infrastructure:Core"), healthChecksBuilder)
                .AddVault(builder.Configuration.GetSection("Infrastructure:Vaults"), healthChecksBuilder)
                .AddSlack(builder.Configuration.GetSection("Infrastructure:DataSources:Slack"), slackConnector)
                .AddGoogle(builder.Configuration.GetSection("Infrastructure:DataSources:Google"), googleConnector)
                .AddMicrosoft(builder.Configuration.GetSection("Infrastructure:DataSources:Microsoft"), office365Connector)
                .AddJira(builder.Configuration.GetSection("Infrastructure:DataSources:Jira"), jiraConnector)
                .AddExcel(builder.Configuration.GetSection("Infrastructure:DataSources:Excel"), excelConnector)
                .AddOrchestratorClient(builder.Configuration.GetSection("Infrastructure:Orchestrator"));

            builder.Services.AddHostedService<StartupHealthChecker>();
            builder.Services.AddHostedService<ConnectionHost>();

            builder.Services.AddApplicationInsightsTelemetryWorkerService();

#if !DEBUG
            builder.Services.RemoveHttpClientLogging();
#endif
            if (builder.Configuration.GetValue<bool>("Infrastructure:Core:Stub"))
            {
                builder.Services.AddNetworkPerspectiveCoreStub(builder.Configuration.GetSection("Infrastructure:Core"));
            }

            var host = builder.Build();
            host.Run();
        }
        catch (Exception)
        {
            var delay = builder.Configuration.GetValue<TimeSpan>("App:DelayBeforeExitOnException");
            Thread.Sleep(delay);
            throw;
        }
    }
}
using Mapster;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Orchestrator.Application;
using NetworkPerspective.Sync.Orchestrator.Controllers;
using NetworkPerspective.Sync.Orchestrator.Extensions;
using NetworkPerspective.Sync.Orchestrator.Hubs.V1;
using NetworkPerspective.Sync.Orchestrator.Hubs.V1.Mappers;
using NetworkPerspective.Sync.Orchestrator.Mappers;
using NetworkPerspective.Sync.Orchestrator.OAuth.Jira;
using NetworkPerspective.Sync.Orchestrator.OAuth.Microsoft;
using NetworkPerspective.Sync.Orchestrator.OAuth.Slack;
using NetworkPerspective.Sync.Orchestrator.Persistence;

namespace NetworkPerspective.Sync.Orchestrator;

public class Program
{
    private static void Main(string[] args)
    {
        ControllersMapsterConfig.RegisterMappings(TypeAdapterConfig.GlobalSettings);
        HubV1MapsterConfig.RegisterMappings(TypeAdapterConfig.GlobalSettings);

        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders().AddConsole();

        var dbConnectionString = builder.Configuration.GetConnectionString("Database");

        builder.Services
            .AddStartupDbInitializer();

        var healthcheckBuilder = builder.Services
            .AddHealthChecks();

        builder.Services
            .AddDocumentation(typeof(Program).Assembly)
            .AddApplication(builder.Configuration.GetSection("App"), dbConnectionString)
            .AddPersistence(healthcheckBuilder)
            .AddAzureKeyVault(builder.Configuration.GetSection("Infrastructure:Vault"), healthcheckBuilder)
            .AddSingleton<ICachedVault, CachedVault>()
            .AddAuth()
            .AddSlackAuth(builder.Configuration.GetSection("DataSources:Slack"))
            .AddMicrosoftAuth()
            .AddJiraAuth(builder.Configuration.GetSection("DataSources:Jira"))
            .AddHub();

        builder.Services.AddControllers(options =>
        {
            options.OutputFormatters.RemoveType<StringOutputFormatter>();
        });

        builder.Services.AddApplicationInsightsTelemetry();

        var app = builder.Build();
        app.UseExceptionHandler(ErrorController.ErrorRoute);
        app.UseRouting();
        app.UseSwagger();
        app.UseSwaggerUi();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapDefaultControllerRoute();
        app.MapHub<WorkerHubV1>("/ws/v1/workers-hub");

        app.Run();
    }
}
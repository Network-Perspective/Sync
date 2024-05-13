using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Framework.Controllers;
using NetworkPerspective.Sync.Orchestrator.Application;
using NetworkPerspective.Sync.Orchestrator.Application.Scheduler;
using NetworkPerspective.Sync.Orchestrator.Extensions;
using NetworkPerspective.Sync.Orchestrator.Hubs;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Impl;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Stub;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Vault.AzureKeyVault;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Vault.Stub;

namespace NetworkPerspective.Sync.Orchestrator;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders().AddConsole();

        var dbConnectionString = builder.Configuration.GetConnectionString("Database");

        builder.Services
            .AddStartupDbInitializer();

        var healthcheckBuilder = builder.Services
            .AddHealthChecks();

        builder.Services
            .AddDocumentation(typeof(Program).Assembly)
            .AddApplication()
            .AddScheduler(builder.Configuration.GetSection("App:Scheduler"), dbConnectionString)
            .AddPersistence(healthcheckBuilder)
            .AddCore(builder.Configuration.GetSection("Infrastructure:Core"), healthcheckBuilder)
            .AddAzureKeyVault(builder.Configuration.GetSection("Infrastructure:Vault"), healthcheckBuilder)
            .AddCoreStub()
            .AddVaultStub()
            .AddAuth()
            .AddHub();

        builder.Services.AddControllers(options =>
        {
            options.OutputFormatters.RemoveType<StringOutputFormatter>();
        });

        var app = builder.Build();
        app.UseExceptionHandler(ErrorController.ErrorRoute);
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapDefaultControllerRoute();
        app.MapHub<WorkerHubV1>("/ws/v1/workers-hub");


        app.Run();
    }
}
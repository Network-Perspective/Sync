using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Hubs;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence;
using NetworkPerspective.Sync.Orchestrator.Application;
using NetworkPerspective.Sync.Orchestrator.Extensions;
using NetworkPerspective.Sync.Infrastructure.Core.Stub;
using Microsoft.Extensions.Configuration;
using NetworkPerspective.Sync.Orchestrator.Application.Scheduler;


var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders().AddConsole();

var dbConnectionString = builder.Configuration.GetConnectionString("Database");

builder.Services
    .AddStartupDbInitializer();

var healthcheckBuilder = builder.Services
    .AddHealthChecks();

builder.Services
    .AddOrchestratorApplication()
    .AddScheduler(builder.Configuration.GetSection("App:Scheduler"), dbConnectionString)
    .AddPersistence(healthcheckBuilder)
    .AddNetworkPerspectiveCoreStub(builder.Configuration.GetSection("Infrastructure:NetworkPerspectiveCore"))
    .AddAuth()
    .AddHub();

builder.Services.AddControllers(options =>
{
    options.OutputFormatters.RemoveType<StringOutputFormatter>();
});  

var app = builder.Build();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultControllerRoute();
app.MapHub<ConnectorHubV1>("/v1/connector-hub");

app.UseHttpsRedirection();

app.Run();

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application;
using NetworkPerspective.Sync.Orchestrator.Application.Scheduler;
using NetworkPerspective.Sync.Orchestrator.Extensions;
using NetworkPerspective.Sync.Orchestrator.Hubs;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Impl;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Stub;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence;


var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders().AddConsole();

var dbConnectionString = builder.Configuration.GetConnectionString("Database");

builder.Services
    .AddStartupDbInitializer();

var healthcheckBuilder = builder.Services
    .AddHealthChecks();

builder.Services
    .AddApplication()
    .AddScheduler(builder.Configuration.GetSection("App:Scheduler"), dbConnectionString)
    .AddPersistence(healthcheckBuilder)
    .AddCore(builder.Configuration.GetSection("Infrastructure:Core"), healthcheckBuilder)
    .AddCoreStub()
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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Hubs;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using System.Threading.Tasks;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Sync;
using System.Threading;
using NetworkPerspective.Sync.Orchestrator.Application;
using NetworkPerspective.Sync.Orchestrator.Extensions;
using NetworkPerspective.Sync.Infrastructure.Core.Stub;
using NetworkPerspective.Sync.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using NetworkPerspective.Sync.Orchestrator.Application.Scheduler;
using NetworkPerspective.Sync.Orchestrator.Application.Domain;


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

builder.Services.AddSingleton<IDataSource, DS>();

var app = builder.Build();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultControllerRoute();
app.MapHub<ConnectorHubV1>("/v1/connector-hub");

app.UseHttpsRedirection();

app.Run();


class DS : IDataSource
{
    public Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        throw new System.NotImplementedException();
    }

    public Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        throw new System.NotImplementedException();
    }

    public Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
    {
        throw new System.NotImplementedException();
    }
}
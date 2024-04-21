using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Framework.Auth;
using NetworkPerspective.Sync.Orchestrator.Hubs;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using System.Threading.Tasks;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Sync;
using System.Threading;
using NetworkPerspective.Sync.Orchestrator.Application;
using NetworkPerspective.Sync.Orchestrator.Extensions;
using NetworkPerspective.Sync.Infrastructure.Core.Stub;


var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders().AddConsole();

builder.Services
    .AddOrchestratorApplication()
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
app.MapHub<ConnectorHubV1>("/connector-hub-v1");

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
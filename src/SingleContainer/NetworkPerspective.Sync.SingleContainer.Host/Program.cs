using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Framework.Auth;
using NetworkPerspective.Sync.Orchestrator.Hubs;
using NetworkPerspective.Sync.Infrastructure.Core.Stub;
using NetworkPerspective.Sync.Application;
using NetworkPerspective.Sync.Framework;
using NetworkPerspective.Sync.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Infrastructure.Persistence;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using System.Threading.Tasks;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Sync;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using NetworkPerspective.Sync.Orchestrator;


var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders().AddConsole();

// Add services to the container.

builder.Services.AddTransient<IUserIdProvider, ConnectorIdProvider>();

builder.Services.AddControllers(options =>
{
    options.OutputFormatters.RemoveType<StringOutputFormatter>();
});

// Framework
builder.Services
    .AddAuthentication(ServiceAuthOptions.DefaultScheme)
    .AddScheme<ServiceAuthOptions, ServiceAuthHandler>(ServiceAuthOptions.DefaultScheme, options => { });

builder.Services
    .AddAuthorization(options =>
    {
        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });
builder.Services
    .AddTransient<IErrorService, ErrorService>();
// end - Framework

builder.Services
    .AddOrchestratorApplication()
    //.AddPersistence()
    //.AddSecretRepositoryClient(builder.Configuration.GetSection("Infrastructure"))
    .AddNetworkPerspectiveCoreStub(builder.Configuration.GetSection("Infrastructure:NetworkPerspectiveCore"));

builder.Services.AddSingleton<IDataSource, DS>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

//builder.Services.RegisterMessageHandlers(typeof(ConnectorPool).Assembly);

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ConnectorHub>("/connector-hub");


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

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
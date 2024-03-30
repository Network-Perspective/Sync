// See https://aka.ms/new-console-template for more information
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.SingleContainer.Connector.Handlers;
using NetworkPerspective.Sync.SingleContainer.Connector.Transport;
using NetworkPerspective.Sync.SingleContainer.Messages;
using NetworkPerspective.Sync.SingleContainer.Messages.Services;

var hubUrl = "http://localhost:5273/connector-hub";
var handshake = new RegisterConnector("Client-123", ConnectorFamily.Excel);


var hubConnection = new HubConnectionBuilder()
    .WithUrl(hubUrl)
    .WithAutomaticReconnect()
    .Build();


var builder = Host.CreateDefaultBuilder();
builder.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<HubConnection>(hubConnection);
    services.AddSingleton<IHostConnectionInternal, HostConnection>();
    services.AddTransient<IHostConnection>(s => s.GetRequiredService<IHostConnectionInternal>());

    services.AddTransient<IMessageSerializer, MessageSerializer>();
    services.AddTransient<IMessageDispatcher, MessageDispatcher>();

    // todo register all handlers via reflection
    services.AddTransient<IRpcHandler<IsAuthenticated, IsAuthenticatedResult>, NetworksHandler>();

    services.RegisterMessageHandlers(typeof(Program).Assembly);
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting connector");
logger.LogInformation("Connecting to hub at {hubUrl}", hubUrl);

var hostConnection = app.Services.GetRequiredService<IHostConnectionInternal>();



hubConnection.On<string, string>("NotifyConnector", (name, payload) =>
{
    using IServiceScope scope = app.Services.CreateScope();
    scope.ServiceProvider
        .GetRequiredService<IMessageDispatcher>()
        .DispatchMessage(name, payload);
});

hubConnection.On<string, string, string, string>("CallConnector", async (correlationId, name, payload, returnType) =>
{
    using IServiceScope scope = app.Services.CreateScope();

    var result = await scope.ServiceProvider
        .GetRequiredService<IRpcDispatcher>()
        .CallRpc(name, payload, returnType);

    await hostConnection.ConnectorReply(correlationId.ToString(), result);
});

hubConnection.On<string, string, string>("HostReply", (name, correlationId, payload) =>
{
    using IServiceScope scope = app.Services.CreateScope();
    scope.ServiceProvider
        .GetRequiredService<IHostConnectionInternal>()
        .HandleHostReply(name, correlationId, payload);
});


hubConnection.Reconnected += async (connectionId) =>
{
    await hostConnection.NotifyAsync(handshake);
};

await hubConnection.StartAsync();

await hostConnection.NotifyAsync(handshake);

// Wait indefinitely
Thread.Sleep(Timeout.Infinite);
// See https://aka.ms/new-console-template for more information
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
    services.AddSingleton<IHostConnection, HostConnection>();

    services.AddTransient<IMessageSerializer, MessageSerializer>();
    services.AddTransient<IMessageDispatcher, MessageDispatcher>();

    services.RegisterMessageHandlers(typeof(Program).Assembly);
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting connector");
logger.LogInformation("Connecting to hub at {hubUrl}", hubUrl);

var hostConnection = app.Services.GetRequiredService<IHostConnection>();



hubConnection.On<string, string>("InvokeConnector", (name, payload) =>
{
    using IServiceScope scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<IMessageDispatcher>().DispatchMessage(name, payload);
});

hubConnection.On<string, string, string>("HostReply", (name, correlationId, payload) =>
{
    using IServiceScope scope = app.Services.CreateScope();
    scope.ServiceProvider
        .GetRequiredService<IHostConnection>()
        .HandleHostReply(name, correlationId, payload);
});


hubConnection.Reconnected += async (connectionId) =>
{
    await hostConnection.InvokeAsync(handshake);
};

await hubConnection.StartAsync();

await hostConnection.InvokeAsync(handshake);

// Wait indefinitely
Thread.Sleep(Timeout.Infinite);
using Microsoft.AspNetCore.Mvc.Formatters;

using NetworkPerspective.Sync.SingleContainer.Host.Impl;
using NetworkPerspective.Sync.SingleContainer.Host.Impl.Transport;
using NetworkPerspective.Sync.SingleContainer.Host.Transport;
using NetworkPerspective.Sync.SingleContainer.Messages.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders().AddConsole();

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.OutputFormatters.RemoveType<StringOutputFormatter>();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// builder.Services.AddTransient<IRemoteConnectorClient, RemoteRemoteConnectorClient>();
builder.Services.RegisterConnectorHostImpl<RemoteConnectorClient>();
builder.Services.RegisterMessageHandlers(typeof(ConnectorPool).Assembly);

var app = builder.Build();

app.MapHub<ConnectorHub>("/connector-hub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.Run();
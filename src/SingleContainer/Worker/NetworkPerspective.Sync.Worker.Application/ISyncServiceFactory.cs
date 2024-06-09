using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Worker.Application;

public interface ISyncServiceFactory
{
    Task<ISyncService> CreateAsync(CancellationToken stoppingToken = default);
}

internal class SyncServiceFactory : ISyncServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public SyncServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<ISyncService> CreateAsync(CancellationToken stoppingToken = default)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        return scope.ServiceProvider.GetRequiredService<ISyncService>();
    }
}

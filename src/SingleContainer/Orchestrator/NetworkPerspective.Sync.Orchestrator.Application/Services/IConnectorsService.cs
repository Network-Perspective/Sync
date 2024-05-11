using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

public interface IConnectorsService
{
    Task AddAsync(Connector connector, CancellationToken cancellationToken);
    Task CreateAsync(Guid id, string type, Guid workerId, IDictionary<string, string> properties, CancellationToken stoppingToken = default);
}

internal class ConnectorsService : IConnectorsService
{
    private readonly ILogger<ConnectorsService> _logger;

    public ConnectorsService(ILogger<ConnectorsService> logger)
    {
        _logger = logger;
    }

    public Task AddAsync(Connector connector, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task CreateAsync(Guid id, string type, Guid workerId, IDictionary<string, string> properties, CancellationToken stoppingToken = default)
    {


        throw new NotImplementedException();
    }
}
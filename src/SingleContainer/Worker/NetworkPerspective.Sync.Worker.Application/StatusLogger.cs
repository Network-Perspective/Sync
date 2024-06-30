using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Contract.V1.Impl;

using ContractStatusLogLevel = NetworkPerspective.Sync.Contract.V1.Dtos.StatusLogLevel;
using DomainStatusLogLevel = NetworkPerspective.Sync.Application.Domain.Statuses.StatusLogLevel;

namespace NetworkPerspective.Sync.Worker.Application;

internal class StatusLogger : IStatusLogger
{
    private readonly ISyncContextAccessor _syncContextAccessor;
    private readonly IWorkerHubClient _hubClient;

    public StatusLogger(ISyncContextAccessor syncContextAccessor, IWorkerHubClient hubClient)
    {
        _syncContextAccessor = syncContextAccessor;
        _hubClient = hubClient;
    }

    public async Task AddLogAsync(string message, DomainStatusLogLevel level, CancellationToken stoppingToken = default)
    {
        var contractLogLevel = level switch
        {
            DomainStatusLogLevel.Error => ContractStatusLogLevel.Error,
            DomainStatusLogLevel.Warning => ContractStatusLogLevel.Warning,
            _ => ContractStatusLogLevel.Info,
        };

        var dto = new AddLogDto
        {
            ConnectorId = _syncContextAccessor.SyncContext.ConnectorId,
            Message = message,
            Level = contractLogLevel,
            CorrelationId = Guid.NewGuid()
        };

        await _hubClient.AddLogAsync(dto);
    }
}
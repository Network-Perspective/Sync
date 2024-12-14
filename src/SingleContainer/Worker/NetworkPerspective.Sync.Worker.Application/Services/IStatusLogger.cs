using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Contract.V1.Impl;

using ContractStatusLogLevel = NetworkPerspective.Sync.Contract.V1.Dtos.StatusLogLevel;
using DomainStatusLogLevel = NetworkPerspective.Sync.Worker.Application.Domain.Statuses.StatusLogLevel;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface IStatusLogger
{
    public Task AddLogAsync(string message, DomainStatusLogLevel level, CancellationToken stoppingToken = default);
}

internal class StatusLogger(IConnectorInfoProvider connectorInfoProvider, IOrchestratorHubClient hubClient) : IStatusLogger
{
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
            ConnectorId = connectorInfoProvider.Get().Id,
            Message = message,
            Level = contractLogLevel,
            CorrelationId = Guid.NewGuid()
        };

        await hubClient.AddLogAsync(dto);
    }
}
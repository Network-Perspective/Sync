using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

using Mapster;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1;
using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Domain.Statuses;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Auth.Worker;
using NetworkPerspective.Sync.Orchestrator.Extensions;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Utils.Models;

namespace NetworkPerspective.Sync.Orchestrator.Hubs.V1;

[Authorize(AuthenticationSchemes = WorkerAuthOptions.DefaultScheme)]
public class WorkerHubV1(IConnectionsLookupTable connectionsLookupTable, IStatusLogger statusLogger, IServiceProvider serviceProvider, IClock clock, ILogger<WorkerHubV1> logger) : Hub<IWorkerClient>, IOrchestratorClient, IWorkerRouter
{
    public override async Task OnConnectedAsync()
    {
        var workerName = Context.GetWorkerName();
        logger.LogInformation("Worker '{name}' connected", workerName);

        var workerConnection = new WorkerConnection(workerName, Context.ConnectionId);
        connectionsLookupTable.Set(workerName, workerConnection);
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        var workerName = Context.GetWorkerName();

        logger.LogInformation("Worker '{name}' disconnected", workerName);
        connectionsLookupTable.Remove(workerName);

        return base.OnDisconnectedAsync(exception);
    }

    public async Task StartSyncAsync(string workerName, SyncContext syncContext)
    {
        var dto = syncContext.Adapt<StartSyncDto>();
        logger.LogInformation("Sending request '{correlationId}' to worker '{id}' to start sync...", dto.CorrelationId, workerName);
        var connection = connectionsLookupTable.Get(workerName);
        var response = await Clients.Client(connection.Id).SyncAsync(dto);
        logger.LogInformation("Received ack '{correlationId}'", response.CorrelationId);
    }

    public async Task SetSecretsAsync(string workerName, IDictionary<string, SecureString> secrets)
    {
        var dto = new SetSecretsDto
        {
            CorrelationId = Guid.NewGuid(),
            Secrets = secrets.ToDictionary(x => x.Key, x => x.Value.ToSystemString())
        };
        logger.LogInformation("Sending request '{correlationId}' to worker '{id}' to set secrets...", dto.CorrelationId, workerName);
        var connection = connectionsLookupTable.Get(workerName);
        var response = await Clients.Client(connection.Id).SetSecretsAsync(dto);
        logger.LogInformation("Received ack '{correlationId}'", response.CorrelationId);
    }

    public async Task RotateSecretsAsync(string workerName, Guid connectorId, IDictionary<string, string> networkProperties, string connectorType)
    {
        var dto = new RotateSecretsDto
        {
            CorrelationId = Guid.NewGuid(),
            ConnectorId = connectorId,
            NetworkProperties = networkProperties,
            ConnectorType = connectorType
        };
        var connection = connectionsLookupTable.Get(workerName);
        var response = await Clients
            .Client(connection.Id)
            .RotateSecretsAsync(dto);
        logger.LogInformation("Received ack '{correlationId}'", response.CorrelationId);
    }

    public async Task<ConnectorStatus> GetConnectorStatusAsync(string workerName, Guid connectorId, Guid networkId, IDictionary<string, string> networkProperties, string connectorType)
    {
        var requestDto = new GetConnectorStatusDto
        {
            CorrelationId = Guid.NewGuid(),
            ConnectorId = connectorId,
            NetworkId = networkId,
            ConnectorType = connectorType,
            ConnectorProperties = networkProperties
        };

        var connection = connectionsLookupTable.Get(workerName);
        var responseDto = await Clients
            .Client(connection.Id)
            .GetConnectorStatusAsync(requestDto);

        if (responseDto.IsRunning)
        {
            var currentTask = ConnectorTaskStatus.Create(responseDto.CurrentTaskCaption, responseDto.CurrentTaskDescription, responseDto.CurrentTaskCompletionRate);
            return ConnectorStatus.Running(responseDto.IsAuthorized, currentTask);
        }
        else
        {
            return ConnectorStatus.Idle(responseDto.IsAuthorized);
        }
    }

    public async Task<AckDto> SyncCompletedAsync(SyncCompletedDto dto)
    {
        logger.LogInformation("Received notification from worker '{id}' sync completed", Context.GetWorkerName());

        var now = clock.UtcNow();
        var timeRange = new TimeRange(dto.Start, dto.End);
        var log = SyncHistoryEntry.Create(dto.ConnectorId, now, timeRange, dto.SuccessRate, dto.TasksCount, dto.TotalInteractionsCount);

        await using var scope = serviceProvider.CreateAsyncScope();
        var syncHistoryService = scope.ServiceProvider.GetService<ISyncHistoryService>();
        await syncHistoryService.SaveLogAsync(log);

        return new AckDto { CorrelationId = dto.CorrelationId };
    }

    public async Task<PongDto> PingAsync(PingDto ping)
    {
        var workerName = Context.GetWorkerName();

        logger.LogInformation("Received ping from {connectorId}", workerName);
        await Task.Yield();
        return new PongDto { CorrelationId = ping.CorrelationId, PingTimestamp = ping.Timestamp };
    }

    public async Task<AckDto> AddLogAsync(AddLogDto dto)
    {
        var domainStatusLogLevel = ToDomainStatusLogLevel(dto.Level);
        await statusLogger.AddLogAsync(dto.ConnectorId, dto.Message, domainStatusLogLevel);
        return new AckDto { CorrelationId = dto.CorrelationId };
    }

    public bool IsConnected(string workerName)
        => connectionsLookupTable.Contains(workerName);

    public async Task<IEnumerable<string>> GetSupportedConnectorTypesAsync(string workerName)
    {
        var requestDto = new GetWorkerCapabilitiesDto
        {
            CorrelationId = Guid.NewGuid()
        };
        var connection = connectionsLookupTable.Get(workerName);
        var capabilities = await Clients
            .Client(connection.Id)
            .GetWorkerCapabilitiesAsync(requestDto);

        return capabilities.SupportedConnectorTypes;
    }

    private static Sync.Application.Domain.Statuses.StatusLogLevel ToDomainStatusLogLevel(StatusLogLevel level)
        => level switch
        {
            StatusLogLevel.Error => Sync.Application.Domain.Statuses.StatusLogLevel.Error,
            StatusLogLevel.Warning => Sync.Application.Domain.Statuses.StatusLogLevel.Warning,
            _ => Sync.Application.Domain.Statuses.StatusLogLevel.Info,
        };
}
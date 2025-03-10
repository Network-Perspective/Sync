﻿using System;
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
        var dto = syncContext.Adapt<SyncRequest>();
        logger.LogInformation("Sending request '{correlationId}' to worker '{id}' to start sync...", dto.CorrelationId, workerName);
        var connection = connectionsLookupTable.Get(workerName);
        var response = await Clients.Client(connection.Id).SyncAsync(dto);
        logger.LogInformation("Received ack '{correlationId}'", response.CorrelationId);
    }

    public async Task SetSecretsAsync(string workerName, IDictionary<string, SecureString> secrets)
    {
        var dto = new SetSecretsRequest
        {
            CorrelationId = Guid.NewGuid(),
            Secrets = secrets.ToDictionary(x => x.Key, x => x.Value.ToSystemString())
        };
        logger.LogInformation("Sending request '{correlationId}' to worker '{id}' to set secrets...", dto.CorrelationId, workerName);
        var connection = connectionsLookupTable.Get(workerName);
        var response = await Clients.Client(connection.Id).SetSecretsAsync(dto);
        logger.LogInformation("Received ack '{correlationId}'", response.CorrelationId);
    }

    public async Task RotateSecretsAsync(string workerName, Guid connectorId, IDictionary<string, string> connectorProperties, string connectorType)
    {
        var dto = new RotateSecretsRequest
        {
            CorrelationId = Guid.NewGuid(),
            Connector = new ConnectorDto
            {
                Id = connectorId,
                Type = connectorType,
                Properties = connectorProperties
            }
        };
        var connection = connectionsLookupTable.Get(workerName);
        var response = await Clients
            .Client(connection.Id)
            .RotateSecretsAsync(dto);
        logger.LogInformation("Received ack '{correlationId}'", response.CorrelationId);
    }

    public async Task<ConnectorStatus> GetConnectorStatusAsync(string workerName, Guid connectorId, Guid networkId, IDictionary<string, string> connectorProperties, string connectorType)
    {
        try
        {
            var requestDto = new ConnectorStatusRequest
            {
                CorrelationId = Guid.NewGuid(),
                Connector = new ConnectorDto
                {
                    Id = connectorId,
                    Type = connectorType,
                    Properties = connectorProperties
                },
            };

            var connection = connectionsLookupTable.Get(workerName);
            var responseDto = await Clients
                .Client(connection.Id)
                .GetConnectorStatusAsync(requestDto);

            return responseDto.Adapt<ConnectorStatus>();
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Error occured while getting status of connector '{connectorId}'", connectorId);
            return ConnectorStatus.Unknown;
        }
    }

    public async Task<OAuthInitializationResult> InitializeOAuthAsync(string workerName, Guid connectorId, string connectorType, string callbackUri, IDictionary<string, string> connectorProperties)
    {
        var requestDto = new InitializeOAuthRequest
        {
            CorrelationId = Guid.NewGuid(),
            Connector = new ConnectorDto
            {
                Id = connectorId,
                Type = connectorType,
                Properties = connectorProperties
            },
            CallbackUri = callbackUri,
        };

        var connection = connectionsLookupTable.Get(workerName);

        var responseDto = await Clients
            .Client(connection.Id)
            .InitializeOAuthAsync(requestDto);

        var result = new OAuthInitializationResult(responseDto.AuthUri, responseDto.State,
            DateTime.SpecifyKind(responseDto.StateExpirationTimestamp, DateTimeKind.Utc));

        return result;
    }

    public async Task HandleOAuthCallbackAsync(string workerName, string code, string state)
    {
        var requestDto = new HandleOAuthCallbackRequest
        {
            CorrelationId = Guid.NewGuid(),
            Code = code,
            State = state
        };

        var connection = connectionsLookupTable.Get(workerName);

        var responseDto = await Clients
            .Client(connection.Id)
            .HandleOAuthCallbackAsync(requestDto);

        logger.LogInformation("Received ack '{correlationId}'", responseDto.CorrelationId);
    }

    public async Task<AckDto> SyncCompletedAsync(SyncResponse dto)
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
        if (dto.ConnectorId == Guid.Empty)
        {
            // TODO worker-scoped logs
            logger.LogWarning("Received request to set worker-scoped status log. Currently only connector-scoped status logs are handled. Igrnoring.");
        }
        else
        {
            var domainStatusLogLevel = ToDomainStatusLogLevel(dto.Level);
            await statusLogger.AddLogAsync(dto.ConnectorId, dto.Message, domainStatusLogLevel);
        }

        return new AckDto { CorrelationId = dto.CorrelationId };
    }

    public bool IsConnected(string workerName)
        => connectionsLookupTable.Contains(workerName);

    public async Task<IEnumerable<string>> GetSupportedConnectorTypesAsync(string workerName)
    {
        var requestDto = new WorkerCapabilitiesRequest
        {
            CorrelationId = Guid.NewGuid()
        };
        var connection = connectionsLookupTable.Get(workerName);
        var capabilities = await Clients
            .Client(connection.Id)
            .GetWorkerCapabilitiesAsync(requestDto);

        return capabilities.SupportedConnectorTypes;
    }

    private static Application.Domain.StatusLogLevel ToDomainStatusLogLevel(Contract.V1.Dtos.StatusLogLevel level)
        => level switch
        {
            Contract.V1.Dtos.StatusLogLevel.Error => Application.Domain.StatusLogLevel.Error,
            Contract.V1.Dtos.StatusLogLevel.Warning => Application.Domain.StatusLogLevel.Warning,
            Contract.V1.Dtos.StatusLogLevel.Debug => Application.Domain.StatusLogLevel.Debug,
            _ => Application.Domain.StatusLogLevel.Info,
        };
}
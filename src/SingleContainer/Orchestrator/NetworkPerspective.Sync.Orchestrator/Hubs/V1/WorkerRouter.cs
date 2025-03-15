using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

using Mapster;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1;
using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Domain.Statuses;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.Hubs.V1;

internal class WorkerRouter(IHubContext<WorkerHubV1, IWorkerClient> hubContext, IConnectionsLookupTable connectionsLookupTable, ILogger<WorkerRouter> logger) : IWorkerRouter
{
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
            var responseDto = await hubContext.Clients
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

    public async Task<IEnumerable<string>> GetSupportedConnectorTypesAsync(string workerName)
    {
        var requestDto = new WorkerCapabilitiesRequest
        {
            CorrelationId = Guid.NewGuid()
        };
        var connection = connectionsLookupTable.Get(workerName);
        var capabilities = await hubContext.Clients
            .Client(connection.Id)
            .GetWorkerCapabilitiesAsync(requestDto);

        return capabilities.SupportedConnectorTypes;
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

        var responseDto = await hubContext.Clients
            .Client(connection.Id)
            .HandleOAuthCallbackAsync(requestDto);

        logger.LogInformation("Received ack '{correlationId}'", responseDto.CorrelationId);
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

        var responseDto = await hubContext.Clients
            .Client(connection.Id)
            .InitializeOAuthAsync(requestDto);

        var result = new OAuthInitializationResult(responseDto.AuthUri, responseDto.State,
            DateTime.SpecifyKind(responseDto.StateExpirationTimestamp, DateTimeKind.Utc));

        return result;
    }

    public bool IsConnected(string workerName)
        => connectionsLookupTable.Contains(workerName);

    public async Task RotateSecretsAsync(string workerName, Guid connectorId, IDictionary<string, string> networkProperties, string connectorType)
    {
        var dto = new RotateSecretsRequest
        {
            CorrelationId = Guid.NewGuid(),
            Connector = new ConnectorDto
            {
                Id = connectorId,
                Type = connectorType,
                Properties = networkProperties
            }
        };
        var connection = connectionsLookupTable.Get(workerName);
        var response = await hubContext.Clients
            .Client(connection.Id)
            .RotateSecretsAsync(dto);
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
        var response = await hubContext.Clients.Client(connection.Id).SetSecretsAsync(dto);
        logger.LogInformation("Received ack '{correlationId}'", response.CorrelationId);
    }

    public async Task StartSyncAsync(string workerName, SyncContext syncContext)
    {
        var dto = syncContext.Adapt<SyncRequest>();
        logger.LogInformation("Sending request '{correlationId}' to worker '{id}' to start sync...", dto.CorrelationId, workerName);
        var connection = connectionsLookupTable.Get(workerName);
        var response = await hubContext.Clients
            .Client(connection.Id)
            .SyncAsync(dto);
        logger.LogInformation("Received ack '{correlationId}'", response.CorrelationId);
    }
}
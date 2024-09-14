using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Domain.Statuses;

namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;

public interface IWorkerRouter
{
    bool IsConnected(string workerName);
    Task StartSyncAsync(string workerName, SyncContext syncContext);
    Task SetSecretsAsync(string workerName, IDictionary<string, SecureString> secrets);
    Task RotateSecretsAsync(string workerName, Guid connectorId, IDictionary<string, string> networkProperties, string connectorType);
    Task<ConnectorStatus> GetConnectorStatusAsync(string workerName, Guid connectorId, Guid networkId, IDictionary<string, string> networkProperties, string connectorType);
}
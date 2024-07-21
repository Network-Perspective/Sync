using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;

namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;

public interface IWorkerRouter
{
    Task StartSyncAsync(string workerName, SyncContext syncContext);
    Task PushSyncAsync(string workerName, SyncRequest syncRequest);
    Task SetSecretsAsync(string workerName, IDictionary<string, SecureString> secrets);
    Task RotateSecretsAsync(string workerName, Guid connectorId, IDictionary<string, string> networkProperties, string connectorType);
}
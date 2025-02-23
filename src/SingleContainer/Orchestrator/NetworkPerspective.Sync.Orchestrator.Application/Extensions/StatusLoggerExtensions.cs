using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Services;

namespace NetworkPerspective.Sync.Orchestrator.Application.Extensions;

public static class StatusLoggerExtensions
{
    public static Task LogErrorAsync(this IStatusLogger service, Guid connectorId, string message, CancellationToken stoppingToken = default)
        => service.AddLogAsync(connectorId, message, StatusLogLevel.Error, stoppingToken);

    public static Task LogWarningAsync(this IStatusLogger service, Guid connectorId, string message, CancellationToken stoppingToken = default)
        => service.AddLogAsync(connectorId, message, StatusLogLevel.Warning, stoppingToken);

    public static Task LogInfoAsync(this IStatusLogger service, Guid connectorId, string message, CancellationToken stoppingToken = default)
        => service.AddLogAsync(connectorId, message, StatusLogLevel.Info, stoppingToken);
}
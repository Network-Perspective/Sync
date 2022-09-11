using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.StatusLogs;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Application.Extensions
{
    public static class StatusLoggerExtensions
    {
        public static Task LogErrorAsync(this IStatusLogger service, Guid networkId, string message, CancellationToken stoppingToken = default)
        {
            var log = StatusLog.Create(networkId, message, StatusLogLevel.Error, DateTime.UtcNow);
            return service.AddLogAsync(log, stoppingToken);
        }

        public static Task LogWarningAsync(this IStatusLogger service, Guid networkId, string message, CancellationToken stoppingToken = default)
        {
            var log = StatusLog.Create(networkId, message, StatusLogLevel.Warning, DateTime.UtcNow);
            return service.AddLogAsync(log, stoppingToken);
        }

        public static Task LogInfoAsync(this IStatusLogger service, Guid networkId, string message, CancellationToken stoppingToken = default)
        {
            var log = StatusLog.Create(networkId, message, StatusLogLevel.Info, DateTime.UtcNow);
            return service.AddLogAsync(log, stoppingToken);
        }
    }
}
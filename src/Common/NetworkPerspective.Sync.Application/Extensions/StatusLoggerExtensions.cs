using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Application.Extensions
{
    public static class StatusLoggerExtensions
    {
        public static Task LogErrorAsync(this IStatusLogger service, string message, CancellationToken stoppingToken = default)
            => service.AddLogAsync(message, StatusLogLevel.Error, stoppingToken);

        public static Task LogWarningAsync(this IStatusLogger service, string message, CancellationToken stoppingToken = default)
            => service.AddLogAsync(message, StatusLogLevel.Warning, stoppingToken);

        public static Task LogInfoAsync(this IStatusLogger service, string message, CancellationToken stoppingToken = default)
            => service.AddLogAsync(message, StatusLogLevel.Info, stoppingToken);
    }
}

using Colors.Net;

using NetworkPerspective.Sync.Infrastructure.Core;

using PowerArgs;

namespace NetworkPerspective.Sync.Cli
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [TabCompletion]
    public class SignalOpts : ICommonOpts
    {
        [ArgDescription("Connector token"), ArgRequired]
        public string Token { get; set; }

        [ArgDescription("Api base url")]
        public string BaseUrl { get; set; }

        [ArgDescription("Action to signal. " +
            "Before uploading data SyncStart should be signaled. " +
            "When sync is completed successfully singal SyncCompleted. " +
            "When there was an error singal SyncError to discard uploaded data and repeat sync process."), ArgRequired]
        public SignalledAction Action { get; set; }

        [ArgDescription("Synchronization period start to announce (yyyy-MM-dd)"), ArgShortcut("S")]
        public string PeriodStart { get; set; }

        [ArgDescription("Synchronization period end to announce (yyyy-MM-dd)"), ArgShortcut("E")]
        public string PeriodEnd { get; set; }

        [ArgDescription("Optional error message in case of signalling error"), ArgShortcut("M")]
        public string ErrorMessage { get; set; }

        [ArgDescription("Timezone (see TimeZoneInfo.FindSystemTimeZoneById)"), ArgRequired, DefaultValue("Central European Standard Time")]
        public string TimeZone { get; set; }
    }

    public enum SignalledAction
    {
        SyncStart, SyncCompleted, SyncError
    }


    public class SignalClient
    {
        private readonly ISyncHashedClient _client;
        private readonly SignalOpts _options;

        public SignalClient(ISyncHashedClient client, SignalOpts options)
        {
            _client = client;
            _options = options;
        }

        public async Task Main()
        {

            DateTimeOffset? periodStart = null;
            DateTimeOffset? periodEnd = null;

            if (!string.IsNullOrEmpty(_options.PeriodStart))
            {
                periodStart = new DateTimeOffset(_options.PeriodStart.AsUtcDate(_options.TimeZone));
            }
            if (!string.IsNullOrEmpty(_options.PeriodEnd))
            {
                periodEnd = new DateTimeOffset(_options.PeriodEnd.AsUtcDate(_options.TimeZone));
            }

            ColoredConsole.WriteLine("Signalling action " + _options.Action);
            ColoredConsole.WriteLine(" - period start: " + periodStart);
            ColoredConsole.WriteLine(" - period end:   " + periodEnd);

            switch (_options.Action)
            {
                case SignalledAction.SyncStart:
                    await _client.ReportStartAsync(new ReportSyncStartedCommand()
                    {
                        ServiceToken = _options.Token,
                        SyncPeriodStart = periodStart,
                        SyncPeriodEnd = periodEnd,
                    });
                    break;
                case SignalledAction.SyncCompleted:
                    await _client.ReportCompletedAsync(new ReportSyncCompletedCommand()
                    {
                        ServiceToken = _options.Token,
                        SyncPeriodStart = periodStart,
                        SyncPeriodEnd = periodEnd,
                        Success = true
                    });
                    break;
                case SignalledAction.SyncError:
                    await _client.ReportCompletedAsync(new ReportSyncCompletedCommand()
                    {
                        ServiceToken = _options.Token,
                        SyncPeriodStart = periodStart,
                        SyncPeriodEnd = periodEnd,
                        Success = false,
                        Message = _options.ErrorMessage
                    });
                    break;
                default:
                    break;
            }

            ColoredConsole.WriteLine("Action signalled successfully");
        }

    }
}
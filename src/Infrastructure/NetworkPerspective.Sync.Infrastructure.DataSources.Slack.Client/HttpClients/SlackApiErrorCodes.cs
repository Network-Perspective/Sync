namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients
{
    public static class SlackApiErrorCodes
    {
        public const string InternalError = "internal_error";
        public const string RequestTimeout = "request_timeout";
        public const string ServiceUnavailable = "service_unavailable";
        public const string FatalError = "fatal_error";
        public const string TokenRevoked = "token_revoked";
    }
}
using System;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients
{
    public class ApiException : Exception
    {
        public int HttpStatusCode { get; }
        public string ApiErrorCode { get; } = string.Empty;

        public ApiException(int httpStatusCode, string apiErrorCode)
            : base($"Response message indicate something went wrong. Error: {apiErrorCode}")
        {
            HttpStatusCode = httpStatusCode;
            ApiErrorCode = apiErrorCode;
        }
    }
}
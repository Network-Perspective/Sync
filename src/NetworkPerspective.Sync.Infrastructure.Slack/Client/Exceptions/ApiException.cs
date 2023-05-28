using System;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Exceptions
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
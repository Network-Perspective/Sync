using System;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Exceptions
{
    public class ApiException : Exception
    {
        public int HttpStatusCode { get; }
        public string ApiErrorCode { get; } = string.Empty;

        public ApiException(int httpStatusCode, Exception innerException)
            : base("Something went wrong during requesting data from Slack Api. Please see inner exception for details", innerException)
        {
            HttpStatusCode = httpStatusCode;
        }

        public ApiException(int httpStatusCode, string apiErrorCode)
            : base($"Response message indicate something went wrong. Error: {apiErrorCode}")
        {
            HttpStatusCode = httpStatusCode;
            ApiErrorCode = apiErrorCode;
        }
    }
}
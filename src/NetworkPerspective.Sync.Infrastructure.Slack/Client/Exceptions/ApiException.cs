using System;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Exceptions
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }

        public ApiException(int statusCode, Exception innerException)
            : base("Something went wrong during requesting data from Slack Api. Please see inner exception for details", innerException)
        {
            StatusCode = statusCode;
        }

        public ApiException(int statusCode, string error)
            : base($"Response message indicate something went wrong. Error: {error}")
        {
            StatusCode = statusCode;
        }
    }
}
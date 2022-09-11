using System;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Exceptions
{
    public class AuthException : Exception
    {
        public AuthException(Exception innerException)
            : base("Something went wrong during authentication process. Please see inner exception for details", innerException)
        { }

        public AuthException(string error)
            : base($"Something went wrong during authentication process. Error: {error}")
        { }
    }
}
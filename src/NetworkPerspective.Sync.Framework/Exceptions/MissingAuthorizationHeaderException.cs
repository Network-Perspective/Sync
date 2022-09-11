using System;

using Microsoft.Net.Http.Headers;

namespace NetworkPerspective.Sync.Framework.Exceptions
{
    public class MissingAuthorizationHeaderException : Exception
    {
        public MissingAuthorizationHeaderException() : base($"The request does not contain {HeaderNames.Authorization} header. " +
            $"Please provide header in format '{HeaderNames.Authorization} Bearer <access_token>'")
        { }
    }
}
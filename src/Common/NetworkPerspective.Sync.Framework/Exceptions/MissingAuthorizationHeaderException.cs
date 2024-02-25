using System;

namespace NetworkPerspective.Sync.Framework.Exceptions
{
    public class MissingAuthorizationHeaderException : Exception
    {
        public MissingAuthorizationHeaderException(string headerName) : base($"The request does not contain '{headerName}' header. " +
            $"Please provide header in format '{headerName} Bearer <access_token>'")
        { }
    }
}
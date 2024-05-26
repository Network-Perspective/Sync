using System;

namespace NetworkPerspective.Sync.Orchestrator.Application.Exceptions;

public class OAuthException : Exception
{
    public string Error { get; }
    public string ErrorDescription { get; }

    public OAuthException(string error)
        : base($"Something went wrong during OAuth process. Error: {error}")
    {
        Error = error;
        ErrorDescription = string.Empty;
    }

    public OAuthException(string error, string errorDescription)
        : base($"Something went wrong during OAuth process. Error: {error} ({errorDescription})")
    {
        Error = error;
        ErrorDescription = errorDescription;
    }
}

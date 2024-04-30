namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Contract.Exceptions;

public class InvalidTokenException : CoreException
{
    public InvalidTokenException(string url)
    : base($"Network Perspective core at '{url}' indicates the access token is NOT valid")
    { }
}

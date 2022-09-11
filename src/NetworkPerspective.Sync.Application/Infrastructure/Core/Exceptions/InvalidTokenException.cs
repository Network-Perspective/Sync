namespace NetworkPerspective.Sync.Application.Infrastructure.Core.Exceptions
{
    public class InvalidTokenException : NetworkPerspectiveCoreException
    {
        public InvalidTokenException(string url)
            : base($"Network Perspective core at '{url}' Indicates the access token is NOT valid")
        { }
    }
}
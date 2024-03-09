namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Exceptions
{
    internal class CannotEveluateTimestampException : MicrosoftException
    {
        public CannotEveluateTimestampException(string message) : base(message)
        { }
    }
}
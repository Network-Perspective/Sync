namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Exceptions
{
    internal class CannotEveluateTimestampException : MicrosoftException
    {
        public CannotEveluateTimestampException(string message) : base(message)
        { }
    }
}
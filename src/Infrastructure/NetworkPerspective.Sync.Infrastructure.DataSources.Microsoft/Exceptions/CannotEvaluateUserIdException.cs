namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Exceptions
{
    internal class CannotEvaluateUserIdException : MicrosoftException
    {
        public CannotEvaluateUserIdException(string message) : base(message)
        { }
    }
}
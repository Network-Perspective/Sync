namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Exceptions
{
    internal class CannotEvaluateUserIdException : MicrosoftException
    {
        public CannotEvaluateUserIdException(string message) : base(message)
        { }
    }
}
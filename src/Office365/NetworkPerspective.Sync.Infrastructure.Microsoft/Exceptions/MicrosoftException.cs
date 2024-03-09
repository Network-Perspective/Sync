using System;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Exceptions
{
    internal class MicrosoftException : Exception
    {
        public MicrosoftException(string message) : base(message)
        { }
    }
}
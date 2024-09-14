using System;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Exceptions
{
    internal class MicrosoftException : Exception
    {
        public MicrosoftException(string message) : base(message)
        { }
    }
}
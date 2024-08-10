using System;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Exceptions
{
    internal class NotSupportedEmailFormatException : Exception
    {
        public string Input { get; }

        public NotSupportedEmailFormatException(string input) :
            base($"Provided input has not supported format: '{input}'")
        {
            Input = input;
        }
    }
}
using System;

namespace NetworkPerspective.Sync.Application.Exceptions
{
    internal class UnexpectedRecurrenceFormatException : Exception
    {
        public UnexpectedRecurrenceFormatException(string rfc5545) : base($"Unexpected RFC5545 string: '{rfc5545}'")
        { }
    }
}
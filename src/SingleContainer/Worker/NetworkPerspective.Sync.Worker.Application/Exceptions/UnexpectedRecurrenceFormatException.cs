namespace NetworkPerspective.Sync.Worker.Application.Exceptions;

internal class UnexpectedRecurrenceFormatException : ApplicationException
{
    public UnexpectedRecurrenceFormatException(string rfc5545) : base($"Unexpected RFC5545 string: '{rfc5545}'")
    { }
}
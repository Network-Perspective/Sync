using System;

namespace NetworkPerspective.Sync.Worker.Application.Exceptions;

public class ApplicationException : Exception
{
    public ApplicationException(string message) : base(message)
    { }
}
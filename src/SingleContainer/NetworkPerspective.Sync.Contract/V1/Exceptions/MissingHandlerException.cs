using System;

namespace NetworkPerspective.Sync.Contract.V1.Exceptions;

public class MissingHandlerException : Exception
{
    public MissingHandlerException(string methodName)
        : base($"Missing handler for method '{methodName}'")
    { }
}
using System;

namespace NetworkPerspective.Sync.Orchestrator.Application.Exceptions;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("Credentials are invalid")
    { }
}
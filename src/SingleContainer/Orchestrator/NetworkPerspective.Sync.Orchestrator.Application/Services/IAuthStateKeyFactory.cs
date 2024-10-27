using System;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

#warning tobedeleted
public interface IAuthStateKeyFactory
{
    string Create();
}

internal class AuthStateKeyFactory : IAuthStateKeyFactory
{
    public string Create()
        => Guid.NewGuid().ToString();
}
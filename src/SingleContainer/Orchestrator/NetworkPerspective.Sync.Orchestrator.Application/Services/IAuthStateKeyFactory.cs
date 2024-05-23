using System;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

public interface IAuthStateKeyFactory
{
    string Create();
}

internal class AuthStateKeyFactory : IAuthStateKeyFactory
{
    public string Create()
        => Guid.NewGuid().ToString();
}
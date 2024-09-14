using System;
using System.Collections.Generic;

using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface IAuthTesterContextFactory
{
    AuthTesterContext Create(Guid connectorId, string connectorType, IDictionary<string, string> properties);
}

internal class AuthTesterContextFactory : IAuthTesterContextFactory
{
    public AuthTesterContext Create(Guid connectorId, string connectorType, IDictionary<string, string> properties)
    {
        return new AuthTesterContext(connectorId, connectorType, properties);
    }
}
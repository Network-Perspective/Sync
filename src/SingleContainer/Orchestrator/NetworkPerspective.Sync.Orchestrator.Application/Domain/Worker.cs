using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkPerspective.Sync.Orchestrator.Application.Domain;

public class Worker
{
    public Guid Id { get; }
    public int Version { get; }
    public string Name { get; }
    public string SecretHash { get; }
    public string SecretSalt { get; }
    public bool IsAuthorized { get; private set; }
    public bool IsOnline { get; private set; }
    public IReadOnlyCollection<string> SupportedConnectorTypes { get; private set; }
    public DateTime CreatedAt { get; }

    public Worker(Guid id, int version, string name, string secretHash, string secretSalt, bool isAuthorized, DateTime createdAt)
    {
        Id = id;
        Version = version;
        Name = name;
        SecretHash = secretHash;
        SecretSalt = secretSalt;
        IsAuthorized = isAuthorized;
        CreatedAt = createdAt;
    }

    public void Authorize()
        => IsAuthorized = true;

    public void SetOnlineStatus(bool isOnline)
        => IsOnline = isOnline;

    public void SetSupportedConnectorTypes(IEnumerable<string> supportedConnectorTypes)
        => SupportedConnectorTypes = supportedConnectorTypes.ToList();
}
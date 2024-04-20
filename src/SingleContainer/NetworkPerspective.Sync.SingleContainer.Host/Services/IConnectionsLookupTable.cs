using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Orchestrator.Services;

public interface IConnectionsLookupTable
{
    string Get(Guid connectorId);
    void Set(Guid connectorId, string connectionId);
    void Remove(Guid connectorId);
}

internal class ConnectionsLookupTable : IConnectionsLookupTable
{
    private readonly IDictionary<Guid, string> _lookupTable = new Dictionary<Guid, string>();
    private readonly object _lock = new();

    public string Get(Guid connectorId)
    {
        lock (_lock)
        {
            return _lookupTable[connectorId];
        }
    }

    public void Set(Guid connectorId, string connectionId)
    {
        lock (_lock)
        {
            _lookupTable[connectorId] = connectionId;
        }
    }

    public void Remove(Guid connectorId)
    {
        lock (_lock)
        {
            _lookupTable.Remove(connectorId);
        }
    }
}

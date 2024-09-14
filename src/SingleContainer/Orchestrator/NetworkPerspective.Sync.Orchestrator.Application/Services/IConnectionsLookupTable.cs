using System.Collections.Generic;

using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

public interface IConnectionsLookupTable
{
    string Get(string workerName);
    bool Contains(string workerName);
    void Set(string workerName, string connectionId);
    void Remove(string workerName);
}

internal class ConnectionsLookupTable : IConnectionsLookupTable
{
    private readonly Dictionary<string, string> _lookupTable = [];
    private readonly object _lock = new();

    public string Get(string workerName)
    {
        lock (_lock)
        {
            return _lookupTable.TryGetValue(workerName, out string value)
                ? value
                : throw new ConnectionNotFoundException(workerName);
        }
    }

    public bool Contains(string workerName)
    {
        lock (_lock)
        {
            return _lookupTable.ContainsKey(workerName);
        }
    }

    public void Set(string workerName, string connectionId)
    {
        lock (_lock)
        {
            _lookupTable[workerName] = connectionId;
        }
    }

    public void Remove(string workerName)
    {
        lock (_lock)
        {
            _lookupTable.Remove(workerName);
        }
    }
}
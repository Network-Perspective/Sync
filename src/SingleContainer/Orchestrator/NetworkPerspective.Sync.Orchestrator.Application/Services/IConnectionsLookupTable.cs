using System.Collections.Generic;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

public interface IConnectionsLookupTable
{
    string Get(string workerName);
    void Set(string workerName, string connectionId);
    void Remove(string workerName);
}

internal class ConnectionsLookupTable : IConnectionsLookupTable
{
    private readonly IDictionary<string, string> _lookupTable = new Dictionary<string, string>();
    private readonly object _lock = new();

    public string Get(string workerName)
    {
        lock (_lock)
        {
            return _lookupTable[workerName];
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
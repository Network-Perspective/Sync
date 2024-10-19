using System.Collections.Generic;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

public interface IConnectionsLookupTable
{
    WorkerConnection Get(string workerName);
    bool Contains(string workerName);
    void Set(string workerName, WorkerConnection connection);
    void Remove(string workerName);
}


internal class ConnectionsLookupTable : IConnectionsLookupTable
{
    private readonly Dictionary<string, WorkerConnection> _lookupTable = [];
    private readonly object _lock = new();

    public WorkerConnection Get(string workerName)
    {
        lock (_lock)
        {
            return _lookupTable.TryGetValue(workerName, out WorkerConnection value)
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

    public void Set(string workerName, WorkerConnection connection)
    {
        lock (_lock)
        {
            _lookupTable[workerName] = connection;
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
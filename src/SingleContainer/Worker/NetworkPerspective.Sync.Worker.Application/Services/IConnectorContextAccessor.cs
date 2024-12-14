using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Exceptions;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface IConnectorContextAccessor
{
    public bool IsAvailable { get; }
    public ConnectorContext Context { get; set; }
}

internal class ConnectorContextAccessor : IConnectorContextAccessor
{
    private readonly object _syncRoot = new();
    private ConnectorContext _context = null;

    public bool IsAvailable
    {
        get
        {
            lock (_syncRoot)
                return _context is not null;
        }
    }

    public ConnectorContext Context
    {
        get
        {
            lock (_syncRoot)
            {
                if (_context is null)
                    throw new ConnectorContextNotAvailableException();

                return _context;
            }
        }
        set
        {
            lock (_syncRoot)
            {
                if (_context is not null)
                    throw new ConnectorContextNotAvailableException();

                _context = value;
            }
        }
    }
}
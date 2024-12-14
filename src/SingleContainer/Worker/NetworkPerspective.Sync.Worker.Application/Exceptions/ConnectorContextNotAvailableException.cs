using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Worker.Application.Exceptions
{
    public class ConnectorContextNotAvailableException : ApplicationException
    {
        public ConnectorContextNotAvailableException() : base($"{nameof(ConnectorContext)} is not avaibale. Make sure You attempt to invoke it in context of Connector")
        { }
    }
}
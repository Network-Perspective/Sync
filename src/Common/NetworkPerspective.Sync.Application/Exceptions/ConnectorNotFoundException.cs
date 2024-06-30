using System;

namespace NetworkPerspective.Sync.Application.Exceptions
{
    public class ConnectorNotFoundException : ApplicationException
    {
        public Guid ConnectorId { get; }

        public ConnectorNotFoundException(Guid id) : base($"Connector '{id}' not found")
        {
            ConnectorId = id;
        }
    }
}
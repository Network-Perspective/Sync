using System;

using NetworkPerspective.Sync.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface IConnectorInfoProvider
    {
        public ConnectorInfo Get();
    }

    public interface IConnectorInfoInitializer
    {
        void Initialize(ConnectorInfo connectorInfo);
    }

    internal class ConnectorInfoProvider : IConnectorInfoProvider, IConnectorInfoInitializer
    {
        private ConnectorInfo _connnectorInfo;

        public ConnectorInfo Get()
            => _connnectorInfo;

        public void Initialize(ConnectorInfo connectorInfo)
            => _connnectorInfo = connectorInfo;
    }
}
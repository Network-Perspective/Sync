using System;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface INetworkIdProvider
    {
        public Guid Get();
    }

    public interface INetworkIdInitializer
    {
        void Initialize(Guid networkId);
    }

    internal class NetworkIdProvider : INetworkIdProvider, INetworkIdInitializer
    {
        private Guid _networkId;

        public Guid Get()
            => _networkId;

        public void Initialize(Guid networkId)
            => _networkId = networkId;
    }
}
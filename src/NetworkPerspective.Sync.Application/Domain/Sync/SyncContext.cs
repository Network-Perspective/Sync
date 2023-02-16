using System;
using System.Collections.Generic;
using System.Security;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Application.Domain.Sync
{
    public class SyncContext : IDisposable
    {
        private readonly IDictionary<Type, object> _container = new Dictionary<Type, object>();
        private readonly IHashingService _hashingService;

        public Guid NetworkId { get; }
        public NetworkConfig NetworkConfig { get; }
        public NetworkProperties NetworkProperties { get; }
        public SecureString AccessToken { get; }
        public TimeRange TimeRange { get; }
        public IStatusLogger StatusLogger { get; }
        public HashFunction HashFunction { get; }

        public SyncContext(Guid networkId, NetworkConfig networkConfig, NetworkProperties networkProperties, SecureString accessToken, TimeRange timeRange, IStatusLogger statusLogger, IHashingService hashingService)
        {
            NetworkId = networkId;
            NetworkConfig = networkConfig;
            NetworkProperties = networkProperties;
            AccessToken = accessToken;
            TimeRange = timeRange;
            StatusLogger = statusLogger;
            _hashingService = hashingService;
            HashFunction = hashingService.Hash;
        }

        public bool Contains<T>()
            => _container.ContainsKey(typeof(T));

        public T Get<T>()
        {
            if (!Contains<T>())
                throw new KeyNotFoundException($"Context does not contain type '{typeof(T)}'");

            return (T)_container[typeof(T)];
        }

        public void Set<T>(T obj)
            => _container[typeof(T)] = obj;

        public void Dispose()
        {
            AccessToken?.Dispose();
            _hashingService?.Dispose();

            foreach (var type in _container.Keys)
            {
                if (type.IsAssignableTo(typeof(IDisposable)))
                    ((IDisposable)_container[type]).Dispose();

                _container.Remove(type);
            }
        }
    }
}
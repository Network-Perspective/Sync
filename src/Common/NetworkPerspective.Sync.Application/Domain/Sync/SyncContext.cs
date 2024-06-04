using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Utils.Models;

namespace NetworkPerspective.Sync.Application.Domain.Sync
{
    public sealed class SyncContext : IDisposable
    {
        private readonly IDictionary<Type, object> _container = new Dictionary<Type, object>();
        private readonly IHashingService _hashingService;

        public Guid ConnectorId { get; }
        public ConnectorConfig NetworkConfig { get; }
        public ConnectorProperties NetworkProperties { get; }
        public SecureString AccessToken { get; }
        public TimeRange TimeRange { get; }
        public IStatusLogger StatusLogger { get; }
        public HashFunction.Delegate HashFunction { get; }

        public SyncContext(Guid connectorId, ConnectorConfig networkConfig, ConnectorProperties networkProperties, SecureString accessToken, TimeRange timeRange, IStatusLogger statusLogger, IHashingService hashingService)
        {
            ConnectorId = connectorId;
            NetworkConfig = networkConfig;
            NetworkProperties = networkProperties;
            AccessToken = accessToken;
            TimeRange = timeRange;
            StatusLogger = statusLogger;
            _hashingService = hashingService;
            HashFunction = hashingService.Hash;
        }


        public T EnsureSet<T>(Func<T> obj)
        {
            if (!_container.ContainsKey(typeof(T)))
                _container[typeof(T)] = obj();

            return (T)_container[typeof(T)];
        }

        public void Set<T>(T obj)
        {
            _container[typeof(T)] = obj;
        }

        public T Get<T>()
        {
            return (T)_container[typeof(T)];
        }

        public async Task<T> EnsureSetAsync<T>(Func<Task<T>> obj)
        {
            if (!_container.ContainsKey(typeof(T)))
                _container[typeof(T)] = await obj();

            return (T)_container[typeof(T)];
        }

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
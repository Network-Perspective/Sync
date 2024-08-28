using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Sync;

public sealed class SyncContext : IDisposable
{
    private readonly IDictionary<Type, object> _container = new Dictionary<Type, object>();
    private readonly IHashingService _hashingService;
    private readonly IEnumerable<KeyValuePair<string, string>> _connectorProperties;

    public Guid ConnectorId { get; }
    public string ConnectorType { get; }
    public ConnectorConfig NetworkConfig { get; }
    public SecureString AccessToken { get; }
    public TimeRange TimeRange { get; }
    public HashFunction.Delegate HashFunction { get; }

    public SyncContext(Guid connectorId, string connectorType, ConnectorConfig networkConfig, IEnumerable<KeyValuePair<string, string>> connectorProperties, SecureString accessToken, TimeRange timeRange, IHashingService hashingService)
    {
        ConnectorId = connectorId;
        ConnectorType = connectorType;
        NetworkConfig = networkConfig;
        _connectorProperties = connectorProperties;
        AccessToken = accessToken;
        TimeRange = timeRange;
        _hashingService = hashingService;
        HashFunction = hashingService.Hash;
    }


    public T GetConnectorProperties<T>() where T : ConnectorProperties, new()
        => ConnectorProperties.Create<T>(_connectorProperties);

    public ConnectorProperties GetConnectorProperties()
        => ConnectorProperties.Create<ConnectorProperties>(_connectorProperties);

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
﻿using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Sync;

public sealed class SyncContext : IDisposable
{
    private readonly Dictionary<Type, object> _container = [];
    public IDictionary<string, string> ConnectorProperties { get; }

    public Guid ConnectorId { get; }
    public string ConnectorType { get; }
    public ConnectorConfig NetworkConfig { get; }
    public SecureString AccessToken { get; }
    public TimeRange TimeRange { get; }

    public SyncContext(Guid connectorId, string connectorType, ConnectorConfig networkConfig, IDictionary<string, string> connectorProperties, SecureString accessToken, TimeRange timeRange)
    {
        ConnectorId = connectorId;
        ConnectorType = connectorType;
        NetworkConfig = networkConfig;
        ConnectorProperties = connectorProperties;
        AccessToken = accessToken;
        TimeRange = timeRange;
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

        foreach (var type in _container.Keys)
        {
            if (type.IsAssignableTo(typeof(IDisposable)))
                ((IDisposable)_container[type]).Dispose();

            _container.Remove(type);
        }
    }
}
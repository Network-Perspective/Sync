using System;

using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface IDataSourceFactory
{
    IDataSource CreateDataSource();
}

internal class DataSourceFactory(ISyncContextAccessor syncContextAccessor, IConnectorTypesCollection connectorTypes, IServiceProvider serviceProvider) : IDataSourceFactory
{
    public IDataSource CreateDataSource()
    {
        var connectorType = syncContextAccessor.SyncContext.ConnectorType;
        var dataSourceFacadeFullName = connectorTypes[connectorType].DataSourceFacadeFullName;
        return serviceProvider.GetRequiredKeyedService<IDataSource>(dataSourceFacadeFullName);
    }
}
using System;

using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Worker.Application.Exceptions;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface IDataSourceFactory
{
    IDataSource CreateDataSource();
}

internal class DataSourceFactory(ISyncContextAccessor syncContextAccessor, IServiceProvider serviceProvider) : IDataSourceFactory
{
    public IDataSource CreateDataSource()
    {
        var type = syncContextAccessor.SyncContext.ConnectorType;

        return ConnectorTypeToDataSource(type);
    }

    private IDataSource ConnectorTypeToDataSource(string connectorType)
    {
        if (string.Equals(connectorType, "Slack"))
            return serviceProvider.GetRequiredKeyedService<IDataSource>("NetworkPerspective.Sync.Infrastructure.DataSources.Slack.SlackFacade");
        else if (string.Equals(connectorType, "Google"))
            return serviceProvider.GetRequiredKeyedService<IDataSource>("NetworkPerspective.Sync.Infrastructure.DataSources.Google.GoogleFacade");
        else if (string.Equals(connectorType, "Excel"))
            return serviceProvider.GetRequiredKeyedService<IDataSource>("NetworkPerspective.Sync.Infrastructure.DataSources.Excel.ExcelFacade");
        else if (string.Equals(connectorType, "Office365"))
            return serviceProvider.GetRequiredKeyedService<IDataSource>("NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.MicrosoftFacade");
        else
            throw new InvalidConnectorTypeException(connectorType);
    }
}
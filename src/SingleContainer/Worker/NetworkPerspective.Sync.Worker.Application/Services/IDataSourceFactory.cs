using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface IDataSourceFactory
{
    IDataSource CreateDataSource();
}

internal class DataSourceFactory : IDataSourceFactory
{
    private readonly ISyncContextAccessor _syncContextAccessor;
    private readonly IEnumerable<IDataSource> _dataSources;

    public DataSourceFactory(ISyncContextAccessor syncContextAccessor, IEnumerable<IDataSource> dataSources)
    {
        _syncContextAccessor = syncContextAccessor;
        _dataSources = dataSources;
    }

    public IDataSource CreateDataSource()
    {
        var type = _syncContextAccessor.SyncContext.ConnectorType;

        return ConnectorTypeToDataSource(type);
    }

    private IDataSource ConnectorTypeToDataSource(string connectorType)
    {
        if (string.Equals(connectorType, "Slack"))
            return _dataSources.Single(x => x.GetType().FullName == "NetworkPerspective.Sync.Infrastructure.Slack.SlackFacade");

        else if (string.Equals(connectorType, "GSuite"))
            return _dataSources.Single(x => x.GetType().FullName == "NetworkPerspective.Sync.Infrastructure.Google.GoogleFacade");

        else if (string.Equals(connectorType, "Excel"))
            return _dataSources.Single(x => x.GetType().FullName == "NetworkPerspective.Sync.Infrastructure.Excel.ExcelFacade");

        else if (string.Equals(connectorType, "Office365"))
            return _dataSources.Single(x => x.GetType().FullName == "NetworkPerspective.Sync.Infrastructure.Microsoft.MicrosoftFacade");

        else
            throw new System.Exception();
    }
}
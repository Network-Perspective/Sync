using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Worker.Application;

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

        else
            throw new System.Exception();
    }
}
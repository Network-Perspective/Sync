using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;

namespace NetworkPerspective.Sync.Worker.Application;

internal class DataSourceFactory : IDataSourceFactory
{
    private readonly IEnumerable<IDataSource> _dataSources;

    public DataSourceFactory(IEnumerable<IDataSource> dataSources)
    {
        _dataSources = dataSources;
    }

    public IDataSource CreateDataSource()
    {
        return _dataSources.Single();
    }
}

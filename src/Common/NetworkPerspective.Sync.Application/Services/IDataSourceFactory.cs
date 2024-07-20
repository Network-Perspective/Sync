using NetworkPerspective.Sync.Application.Infrastructure.DataSources;

namespace NetworkPerspective.Sync.Application.Services;

public interface IDataSourceFactory
{
    IDataSource CreateDataSource();
}

internal class DefaultDataSourceFactory(IDataSource dataSource) : IDataSourceFactory
{
    public IDataSource CreateDataSource()
    {
        return dataSource;
    }
}
namespace NetworkPerspective.Sync.Application.Infrastructure.DataSources;

public interface IDataSourceFactory
{
    IDataSource CreateDataSource();
}

internal class DataSourceFactory : IDataSourceFactory
{
    private readonly IDataSource _dataSource;

    public DataSourceFactory(IDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public IDataSource CreateDataSource()
    {
        return _dataSource;
    }
}
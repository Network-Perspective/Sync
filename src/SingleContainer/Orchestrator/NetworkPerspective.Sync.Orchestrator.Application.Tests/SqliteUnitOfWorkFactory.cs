using System;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Orchestrator.Application.Tests;

public class SqliteUnitOfWorkFactory : IUnitOfWorkFactory, IDisposable
{
    private const string ConnectionStringTemplate = "DataSource=file:{0}?mode=memory&cache=shared";
    private readonly DbContextOptions<OrchestratorDbContext> _options;
    private readonly SqliteConnection _keepAliveConnection;

    public SqliteUnitOfWorkFactory()
    {
        var connectionString = string.Format(ConnectionStringTemplate, Guid.NewGuid());

        _options = new DbContextOptionsBuilder<OrchestratorDbContext>()
          .UseSqlite(connectionString)
          .EnableSensitiveDataLogging()
          .Options;

        using var dbContext = new OrchestratorDbContext(_options);

        dbContext.Database.EnsureCreated();

        _keepAliveConnection = new SqliteConnection(connectionString);
        _keepAliveConnection.Open();
    }

    public IUnitOfWork Create()
        => new UnitOfWork(_options);

    public void Dispose()
    {
        _keepAliveConnection.Close();
        _keepAliveConnection.Dispose();
    }
}
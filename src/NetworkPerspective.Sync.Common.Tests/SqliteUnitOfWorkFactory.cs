using System;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Common.Tests
{
    public class SqliteUnitOfWorkFactory : IUnitOfWorkFactory, IDisposable
    {
        private const string ConnectionStringTemplate = "DataSource=file:{0}?mode=memory&cache=shared";
        private readonly DbContextOptions<ConnectorDbContext> _options;
        private readonly SqliteConnection keepAliveConnection;

        public SqliteUnitOfWorkFactory()
        {
            var connectionString = string.Format(ConnectionStringTemplate, Guid.NewGuid());

            _options = new DbContextOptionsBuilder<ConnectorDbContext>()
              .UseSqlite(connectionString)
              .EnableSensitiveDataLogging()
              .Options;

            using var dbContext = new ConnectorDbContext(_options);

            dbContext.Database.EnsureCreated();

            keepAliveConnection = new SqliteConnection(connectionString);
            keepAliveConnection.Open();
        }

        public IUnitOfWork Create()
            => new UnitOfWork(_options);

        public void Dispose()
        {
            keepAliveConnection.Close();
            keepAliveConnection.Dispose();
        }
    }
}
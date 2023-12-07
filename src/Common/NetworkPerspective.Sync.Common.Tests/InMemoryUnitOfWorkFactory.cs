using System;

using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Common.Tests
{
    public class InMemoryUnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly DbContextOptions<ConnectorDbContext> _options;

        public InMemoryUnitOfWorkFactory()
        {
            _options = new DbContextOptionsBuilder<ConnectorDbContext>()
              .UseInMemoryDatabase(Guid.NewGuid().ToString())
              .EnableSensitiveDataLogging()
              .Options;

            var dbContext = new ConnectorDbContext(_options);

            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        }

        public IUnitOfWork Create()
            => new UnitOfWork(_options);
    }
}
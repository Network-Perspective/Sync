using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence;

public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly DbContextOptions<ConnectorDbContext> _options;

    public UnitOfWorkFactory(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        var migrationsAssemblyName = typeof(DesignTimeDbContextFactory).Assembly.GetName().Name;
        _options = new DbContextOptionsBuilder<ConnectorDbContext>()
          .UseSqlServer(connectionString, x => x.MigrationsAssembly(migrationsAssemblyName))
          .Options;
    }

    public IUnitOfWork Create()
        => new UnitOfWork(_options);
}
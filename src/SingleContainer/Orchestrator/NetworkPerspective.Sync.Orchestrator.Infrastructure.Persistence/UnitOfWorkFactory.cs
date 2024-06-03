using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Migrations;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence;

public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly DbContextOptions<OrchestratorDbContext> _options;

    public UnitOfWorkFactory(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        var migrationsAssemblyName = typeof(DesignTimeDbContextFactory).Assembly.GetName().Name;
        _options = new DbContextOptionsBuilder<OrchestratorDbContext>()
          .UseSqlServer(connectionString, x => x.MigrationsAssembly(migrationsAssemblyName))
          .Options;
    }

    public IUnitOfWork Create()
        => new UnitOfWork(_options);
}
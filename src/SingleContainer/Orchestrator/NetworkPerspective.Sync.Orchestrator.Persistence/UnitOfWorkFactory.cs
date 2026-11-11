using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Orchestrator.Persistence.Migrations;

namespace NetworkPerspective.Sync.Orchestrator.Persistence;

public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly DbContextOptions<OrchestratorDbContext> _options;

    public UnitOfWorkFactory(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        _options = CreateOptions(configuration.GetDatabaseProvider(), connectionString);
    }

    public static DbContextOptions<OrchestratorDbContext> CreateOptions(DatabaseProvider provider, string connectionString)
    {
        var builder = new DbContextOptionsBuilder<OrchestratorDbContext>();
        if (provider == DatabaseProvider.PostgreSql)
            builder.UseNpgsql(connectionString);
        else if (provider == DatabaseProvider.SqlServer)
            builder.UseSqlServer(connectionString, x => x.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));
        else
            throw new System.ArgumentOutOfRangeException(nameof(provider));

        return builder.Options;
    }

    public IUnitOfWork Create()
        => new UnitOfWork(_options);
}
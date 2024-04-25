using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence;

public class ConnectorDbContext : DbContext
{
    public DbSet<SyncHistoryEntryEntity> SyncHistoryEntities { get; set; }
    public DbSet<DataSourceEntity> NetworkEntities { get; set; }
    public DbSet<StatusLogEntity> StatusLogEntities { get; set; }
    public DbSet<SecretEntity> SecretEntities { get; set; }

    public ConnectorDbContext(DbContextOptions<ConnectorDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConnectorDbContext).Assembly);
}
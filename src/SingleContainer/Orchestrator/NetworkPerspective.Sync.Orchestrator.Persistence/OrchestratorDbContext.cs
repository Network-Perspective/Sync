using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Orchestrator.Persistence.Entities;

namespace NetworkPerspective.Sync.Orchestrator.Persistence;

public class OrchestratorDbContext : DbContext
{
    public DbSet<SyncHistoryEntryEntity> SyncHistoryEntities { get; set; }
    public DbSet<ConnectorEntity> ConnectorEntities { get; set; }
    public DbSet<ConnectorPropertyEntity> ConnectorPropertyEntities { get; set; }
    public DbSet<WorkerEntity> WorkerEntities { get; set; }
    public DbSet<StatusLogEntity> StatusLogEntities { get; set; }
    public DbSet<SecretEntity> SecretEntities { get; set; }

    public OrchestratorDbContext(DbContextOptions<OrchestratorDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrchestratorDbContext).Assembly);
}
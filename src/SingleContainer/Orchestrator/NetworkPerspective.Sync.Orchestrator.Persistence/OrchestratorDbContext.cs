using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrchestratorDbContext).Assembly);

        if (Database.IsNpgsql())
        {
            // SQL Server returned Unspecified values; the application's persisted timestamps represent UTC.
            var utc = new ValueConverter<DateTime, DateTime>(
                value => value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : DateTime.SpecifyKind(value, DateTimeKind.Utc),
                value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
                foreach (var property in entity.GetProperties())
                    if (property.ClrType == typeof(DateTime))
                        property.SetValueConverter(utc);
        }
    }
}
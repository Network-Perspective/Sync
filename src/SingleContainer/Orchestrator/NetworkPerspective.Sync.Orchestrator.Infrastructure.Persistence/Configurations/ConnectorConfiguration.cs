using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Configurations;

public class ConnectorConfiguration : IEntityTypeConfiguration<ConnectorEntity>
{
    public void Configure(EntityTypeBuilder<ConnectorEntity> builder)
    {
        builder
            .ToTable("Connectors");

        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.CreatedAt)
            .IsRequired(true);

        builder
            .HasMany(x => x.Properties)
            .WithOne(x => x.Connector)
            .HasForeignKey(x => x.ConnectorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.SyncHistory)
            .WithOne(x => x.Connector)
            .HasForeignKey(x => x.ConnectorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
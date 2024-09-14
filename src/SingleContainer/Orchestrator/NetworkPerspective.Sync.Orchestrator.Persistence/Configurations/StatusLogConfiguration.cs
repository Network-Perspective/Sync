using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NetworkPerspective.Sync.Orchestrator.Persistence.Entities;

namespace NetworkPerspective.Sync.Orchestrator.Persistence.Configurations;

public class StatusLogConfiguration : IEntityTypeConfiguration<StatusLogEntity>
{
    public void Configure(EntityTypeBuilder<StatusLogEntity> builder)
    {
        builder
            .ToTable("StatusLogs");

        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.ConnectorId)
            .IsRequired(true);

        builder
            .Property(x => x.TimeStamp)
            .IsRequired(true);

        builder
            .Property(x => x.Message)
            .HasMaxLength(1024)
            .IsRequired(true);

        builder
            .Property(x => x.Level)
            .HasMaxLength(1024)
            .IsRequired(true);

        builder
            .HasIndex(x => x.ConnectorId);
    }
}
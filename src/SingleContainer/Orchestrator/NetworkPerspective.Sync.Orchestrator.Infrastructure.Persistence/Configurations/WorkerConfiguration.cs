using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Configurations;

public class WorkerConfiguration : IEntityTypeConfiguration<WorkerEntity>
{
    public void Configure(EntityTypeBuilder<WorkerEntity> builder)
    {
        builder
            .ToTable("Workers");

        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.CreatedAt)
            .IsRequired(true);

        builder
            .HasMany(x => x.Connectors)
            .WithOne(x => x.Worker)
            .HasForeignKey(x => x.WorkerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
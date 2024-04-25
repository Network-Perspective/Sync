using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Configurations;

public class DataSourceConfiguration : IEntityTypeConfiguration<DataSourceEntity>
{
    public void Configure(EntityTypeBuilder<DataSourceEntity> builder)
    {
        builder
            .ToTable("DataSources");

        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.CreatedAt)
            .IsRequired(true);

        builder
            .HasMany(x => x.Properties)
            .WithOne(x => x.DataSource)
            .HasForeignKey(x => x.DataSourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.SyncHistory)
            .WithOne(x => x.DataSource)
            .HasForeignKey(x => x.DataSourceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
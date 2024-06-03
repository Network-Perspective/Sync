using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Configurations;

public class SyncHistoryConfiguration : IEntityTypeConfiguration<SyncHistoryEntryEntity>
{
    public void Configure(EntityTypeBuilder<SyncHistoryEntryEntity> builder)
    {
        builder
            .ToTable("SyncHistory");

        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.TimeStamp);

        builder
            .Property(x => x.SyncPeriodStart);

        builder
            .Property(x => x.SyncPeriodEnd);

        builder
            .Property(x => x.SuccessRate)
            .IsRequired(false);

        builder
            .Property(x => x.TasksCount);

        builder
            .Property(x => x.InteractionsCount);
    }
}
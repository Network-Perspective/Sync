using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NetworkPerspective.Sync.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Configurations
{
    public class NetworkConfiguration : IEntityTypeConfiguration<NetworkEntity>
    {
        public void Configure(EntityTypeBuilder<NetworkEntity> builder)
        {
            builder
                .ToTable("Networks");

            builder
                .HasKey(x => x.Id);

            builder
                .Property(x => x.CreatedAt)
                .IsRequired(true);

            builder
                .HasMany(x => x.Properties)
                .WithOne(x => x.Network)
                .HasForeignKey(x => x.NetworkId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(x => x.SyncHistory)
                .WithOne(x => x.Network)
                .HasForeignKey(x => x.NetworkId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
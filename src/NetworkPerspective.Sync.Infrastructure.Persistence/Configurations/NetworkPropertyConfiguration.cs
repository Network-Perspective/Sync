using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NetworkPerspective.Sync.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Configurations
{
    public class NetworkPropertyConfiguration : IEntityTypeConfiguration<NetworkPropertyEntity>
    {
        public void Configure(EntityTypeBuilder<NetworkPropertyEntity> builder)
        {
            builder
                .ToTable("NetworkProperties");

            builder
                .HasKey(x => x.Id);

            builder
                .Property(x => x.Key)
                .HasMaxLength(256)
                .IsRequired(true);

            builder
                .Property(x => x.Value)
                .HasMaxLength(1024)
                .IsRequired(true);
        }
    }
}
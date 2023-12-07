using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NetworkPerspective.Sync.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Configurations
{
    public class DbSecretsConfiguration : IEntityTypeConfiguration<SecretEntity>
    {
        public void Configure(EntityTypeBuilder<SecretEntity> builder)
        {
            builder
                .ToTable("DbSecrets");

            builder
                .Property(x => x.Key)
                .IsRequired(true);

            builder
                .Property(x => x.Value)
                .IsRequired(true);
        }
    }
}
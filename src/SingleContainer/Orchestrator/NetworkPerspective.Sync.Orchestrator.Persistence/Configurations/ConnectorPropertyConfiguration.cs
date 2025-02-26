﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NetworkPerspective.Sync.Orchestrator.Persistence.Entities;

namespace NetworkPerspective.Sync.Orchestrator.Persistence.Configurations;

public class ConnectorPropertyConfiguration : IEntityTypeConfiguration<ConnectorPropertyEntity>
{
    public void Configure(EntityTypeBuilder<ConnectorPropertyEntity> builder)
    {
        builder
            .ToTable("ConnectorProperties");

        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Key)
            .HasMaxLength(256)
            .IsRequired(true);

        builder
            .Property(x => x.Value)
            .IsRequired(true);
    }
}
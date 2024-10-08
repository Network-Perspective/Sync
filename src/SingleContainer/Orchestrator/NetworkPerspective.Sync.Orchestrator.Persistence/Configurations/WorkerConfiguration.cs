﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NetworkPerspective.Sync.Orchestrator.Persistence.Entities;

namespace NetworkPerspective.Sync.Orchestrator.Persistence.Configurations;

public class WorkerConfiguration : IEntityTypeConfiguration<WorkerEntity>
{
    public void Configure(EntityTypeBuilder<WorkerEntity> builder)
    {
        builder
            .ToTable("Workers");

        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Version)
            .IsRequired();

        builder
            .HasIndex(x => x.Name)
            .IsUnique();

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
﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetworkPerspective.Sync.Orchestrator.Persistence;

#nullable disable

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(OrchestratorDbContext))]
    partial class OrchestratorDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Persistence.Entities.ConnectorEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("NetworkId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("WorkerId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("WorkerId");

                    b.ToTable("Connectors", (string)null);
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Persistence.Entities.ConnectorPropertyEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<Guid>("ConnectorId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ConnectorId");

                    b.ToTable("ConnectorProperties", (string)null);
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Persistence.Entities.SecretEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("DbSecrets", (string)null);
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Persistence.Entities.StatusLogEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<Guid>("ConnectorId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Level")
                        .HasMaxLength(1024)
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("ConnectorId");

                    b.ToTable("StatusLogs", (string)null);
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Persistence.Entities.SyncHistoryEntryEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<Guid>("ConnectorId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("InteractionsCount")
                        .HasColumnType("bigint");

                    b.Property<double?>("SuccessRate")
                        .HasColumnType("float");

                    b.Property<DateTime>("SyncPeriodEnd")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("SyncPeriodStart")
                        .HasColumnType("datetime2");

                    b.Property<int>("TasksCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("ConnectorId");

                    b.ToTable("SyncHistory", (string)null);
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Persistence.Entities.WorkerEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsAuthorized")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("SecretHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SecretSalt")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Version")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasFilter("[Name] IS NOT NULL");

                    b.ToTable("Workers", (string)null);
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Persistence.Entities.ConnectorEntity", b =>
                {
                    b.HasOne("NetworkPerspective.Sync.Orchestrator.Persistence.Entities.WorkerEntity", "Worker")
                        .WithMany("Connectors")
                        .HasForeignKey("WorkerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Worker");
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Persistence.Entities.ConnectorPropertyEntity", b =>
                {
                    b.HasOne("NetworkPerspective.Sync.Orchestrator.Persistence.Entities.ConnectorEntity", "Connector")
                        .WithMany("Properties")
                        .HasForeignKey("ConnectorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Connector");
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Persistence.Entities.StatusLogEntity", b =>
                {
                    b.HasOne("NetworkPerspective.Sync.Orchestrator.Persistence.Entities.ConnectorEntity", "Connector")
                        .WithMany()
                        .HasForeignKey("ConnectorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Connector");
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Persistence.Entities.SyncHistoryEntryEntity", b =>
                {
                    b.HasOne("NetworkPerspective.Sync.Orchestrator.Persistence.Entities.ConnectorEntity", "Connector")
                        .WithMany("SyncHistory")
                        .HasForeignKey("ConnectorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Connector");
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Persistence.Entities.ConnectorEntity", b =>
                {
                    b.Navigation("Properties");

                    b.Navigation("SyncHistory");
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Persistence.Entities.WorkerEntity", b =>
                {
                    b.Navigation("Connectors");
                });
#pragma warning restore 612, 618
        }
    }
}

﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence;

#nullable disable

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(ConnectorDbContext))]
    [Migration("20240429193941_Quartz")]
    partial class Quartz
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities.DataSourceEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ConnectorId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("NetworkId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("DataSources", (string)null);
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities.DataSourcePropertyEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<Guid>("DataSourceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.HasKey("Id");

                    b.HasIndex("DataSourceId");

                    b.ToTable("DataSourceProperties", (string)null);
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities.SecretEntity", b =>
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

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities.StatusLogEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<Guid>("DataSourceId")
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

                    b.HasIndex("DataSourceId");

                    b.ToTable("StatusLogs", (string)null);
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities.SyncHistoryEntryEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<Guid>("DataSourceId")
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

                    b.HasIndex("DataSourceId");

                    b.ToTable("SyncHistory", (string)null);
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities.DataSourcePropertyEntity", b =>
                {
                    b.HasOne("NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities.DataSourceEntity", "DataSource")
                        .WithMany("Properties")
                        .HasForeignKey("DataSourceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DataSource");
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities.StatusLogEntity", b =>
                {
                    b.HasOne("NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities.DataSourceEntity", "DataSource")
                        .WithMany()
                        .HasForeignKey("DataSourceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DataSource");
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities.SyncHistoryEntryEntity", b =>
                {
                    b.HasOne("NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities.DataSourceEntity", "DataSource")
                        .WithMany("SyncHistory")
                        .HasForeignKey("DataSourceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DataSource");
                });

            modelBuilder.Entity("NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities.DataSourceEntity", b =>
                {
                    b.Navigation("Properties");

                    b.Navigation("SyncHistory");
                });
#pragma warning restore 612, 618
        }
    }
}

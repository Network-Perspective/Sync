using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Orchestrator.Application;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Orchestrator.Persistence.Entities;
using NetworkPerspective.Sync.Orchestrator.Persistence.Schema;

using Npgsql;

using Quartz;

using Testcontainers.PostgreSql;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Persistence.Tests;

public class PostgreSqlLocalIntegrationTests
{
    [SkippableFact]
    public async Task FreshSchemaSyntheticRoundTripAndPersistentQuartz()
    {
        Skip.If(Environment.GetEnvironmentVariable("SYNC_OVH_LOCAL_POSTGRES") != "true",
            "Opt in explicitly after pulling postgres:18 on the local Docker daemon.");
        Environment.GetEnvironmentVariable("DOCKER_HOST").Should().Be("unix:///var/run/docker.sock",
            "this test must never select a remote Docker context");

        await using var database = new PostgreSqlBuilder()
            .WithImage("postgres:18")
            .WithDatabase("sync_synthetic")
            .WithUsername("sync_synthetic")
            .WithPassword("synthetic-local-only-password")
            .Build();
        await database.StartAsync();
        var connectionString = database.GetConnectionString();

        await using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(PostgreSqlSchema.GenerateScript(), connection);
            await command.ExecuteNonQueryAsync();
            Func<Task> repeat = async () => await command.ExecuteNonQueryAsync();
            await repeat.Should().ThrowAsync<PostgresException>();
        }

        var options = UnitOfWorkFactory.CreateOptions(DatabaseProvider.PostgreSql, connectionString);
        var workerId = Guid.NewGuid();
        var connectorId = Guid.NewGuid();
        var date = new DateTime(2026, 1, 2, 3, 4, 5);
        await using (var context = new OrchestratorDbContext(options))
        {
            context.WorkerEntities.Add(new WorkerEntity
            {
                Id = workerId,
                Name = "synthetic-worker",
                SecretHash = "synthetic-hash",
                SecretSalt = "synthetic-salt",
                Version = 1,
                IsAuthorized = true,
                CreatedAt = date
            });
            context.ConnectorEntities.Add(new ConnectorEntity
            {
                Id = connectorId,
                WorkerId = workerId,
                NetworkId = Guid.NewGuid(),
                Type = "synthetic",
                CreatedAt = date
            });
            context.ConnectorPropertyEntities.Add(new ConnectorPropertyEntity
            {
                ConnectorId = connectorId,
                Key = "synthetic",
                Value = "Zażółć gęślą jaźń 🧪"
            });
            context.SyncHistoryEntities.Add(new SyncHistoryEntryEntity
            {
                ConnectorId = connectorId,
                TimeStamp = date,
                SyncPeriodStart = date,
                SyncPeriodEnd = date.AddHours(1),
                SuccessRate = 0.75,
                TasksCount = 4,
                InteractionsCount = 42
            });
            context.StatusLogEntities.Add(new StatusLogEntity { ConnectorId = connectorId, TimeStamp = date, Message = "synthetic", Level = 1 });
            context.SecretEntities.Add(new SecretEntity { Id = Guid.NewGuid(), Key = "synthetic", Value = "synthetic-encrypted-placeholder" });
            await context.SaveChangesAsync();
        }
        await using (var context = new OrchestratorDbContext(options))
        {
            var worker = await context.WorkerEntities.SingleAsync();
            worker.Id.Should().Be(workerId);
            worker.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
            worker.CreatedAt.Ticks.Should().Be(date.Ticks);
            (await context.ConnectorPropertyEntities.SingleAsync()).Value.Should().Be("Zażółć gęślą jaźń 🧪");
            (await context.SyncHistoryEntities.SingleAsync()).InteractionsCount.Should().Be(42);
            (await context.StatusLogEntities.SingleAsync()).Id.Should().BePositive();
            (await context.SecretEntities.SingleAsync()).Value.Should().Be("synthetic-encrypted-placeholder");
            await context.ConnectorPropertyEntities.ExecuteDeleteAsync();
            await context.WorkerEntities.ExecuteDeleteAsync();
            (await context.ConnectorEntities.CountAsync()).Should().Be(0);
            (await context.SyncHistoryEntities.CountAsync()).Should().Be(0);
        }

        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["Scheduler:UsePersistentStore"] = "true"
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication(config, connectionString, DatabaseProvider.PostgreSql);
        await using var provider = services.BuildServiceProvider();
        var scheduler = await provider.GetRequiredService<ISchedulerFactory>().GetScheduler();
        try
        {
            var job = JobBuilder.Create<SyntheticJob>().WithIdentity("synthetic-only").StoreDurably().Build();
            await scheduler.AddJob(job, false);
            (await scheduler.GetJobDetail(job.Key)).Should().NotBeNull();
            await scheduler.Start();
            await scheduler.TriggerJob(job.Key);
            await SyntheticJob.Executed.Task.WaitAsync(TimeSpan.FromSeconds(20));
        }
        finally
        {
            await scheduler.Shutdown(true);
        }
    }

    public class SyntheticJob : IJob
    {
        public static readonly TaskCompletionSource<bool> Executed = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Execute(IJobExecutionContext context)
        {
            Executed.TrySetResult(true);
            return Task.CompletedTask;
        }
    }
}
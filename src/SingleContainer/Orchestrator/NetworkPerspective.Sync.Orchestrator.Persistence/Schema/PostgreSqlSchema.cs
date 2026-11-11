using System.IO;

using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Orchestrator.Persistence.Schema;

public static class PostgreSqlSchema
{
    // Emits SQL only: no connection, migration, seed, scheduler or vault access.
    public static string GenerateScript()
    {
        using var context = new OrchestratorDbContext(UnitOfWorkFactory.CreateOptions(DatabaseProvider.PostgreSql, "Host=localhost;Database=schema_generation_only"));
        using var quartz = typeof(PostgreSqlSchema).Assembly.GetManifestResourceStream(
            "NetworkPerspective.Sync.Orchestrator.Persistence.Schema.Quartz_PostgreSql.sql");
        using var reader = new StreamReader(quartz);
        return "-- EMPTY, separately approved Sync database only. Never import SQL Server Quartz rows.\nBEGIN;\n"
            + context.Database.GenerateCreateScript() + "\n" + reader.ReadToEnd() + "\nCOMMIT;\n";
    }
}
using System;

using Microsoft.Extensions.Configuration;

namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;

public enum DatabaseProvider
{
    SqlServer,
    PostgreSql
}

public static class DatabaseConfiguration
{
    public static DatabaseProvider GetDatabaseProvider(this IConfiguration configuration)
    {
        var value = configuration["Database:Provider"] ?? nameof(DatabaseProvider.SqlServer);
        if (string.Equals(value, nameof(DatabaseProvider.SqlServer), StringComparison.OrdinalIgnoreCase))
            return DatabaseProvider.SqlServer;
        if (string.Equals(value, nameof(DatabaseProvider.PostgreSql), StringComparison.OrdinalIgnoreCase))
            return DatabaseProvider.PostgreSql;

        throw new InvalidOperationException("Database:Provider must be SqlServer or PostgreSql.");
    }
}
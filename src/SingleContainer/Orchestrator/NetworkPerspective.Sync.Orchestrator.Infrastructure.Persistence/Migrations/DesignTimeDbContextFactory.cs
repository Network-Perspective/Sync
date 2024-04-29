using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Migrations
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ConnectorDbContext>
    {
        public ConnectorDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ConnectorDbContext>();
            var connectionString = GetConnectionString();
            var migrationsAssemblyName = GetType().GetTypeInfo().Assembly.GetName().Name;
            optionsBuilder.UseSqlServer(connectionString, x => x.MigrationsAssembly(migrationsAssemblyName));

            return new ConnectorDbContext(optionsBuilder.Options);
        }

        private string GetConnectionString()
        {
            return "Data Source = .\\SQLEXPRESS; Initial Catalog = Test-NetworkPerspective.Orchestrator; Integrated Security = True; TrustServerCertificate = True";
        }
    }
}
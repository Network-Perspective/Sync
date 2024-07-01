using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Migrations
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OrchestratorDbContext>
    {
        public OrchestratorDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OrchestratorDbContext>();
            var connectionString = GetConnectionString();
            var migrationsAssemblyName = GetType().GetTypeInfo().Assembly.GetName().Name;
            optionsBuilder.UseSqlServer(connectionString, x => x.MigrationsAssembly(migrationsAssemblyName));

            return new OrchestratorDbContext(optionsBuilder.Options);
        }

        private string GetConnectionString()
        {
            return "Data Source = .\\SQLEXPRESS; Initial Catalog = Test-NetworkPerspective.Orchestrator; Integrated Security = True; TrustServerCertificate = True";
        }
    }
}
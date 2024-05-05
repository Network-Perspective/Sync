using System;
using System.IO;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Quartz : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "Migrations", "Scripts", "Quartz_SqlServer_up.sql");
            var scriptContent = File.ReadAllText(scriptPath);
            migrationBuilder.Sql(scriptContent);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "Migrations", "Scripts", "Quartz_SqlServer_down.sql");
            var scriptContent = File.ReadAllText(scriptPath);
            migrationBuilder.Sql(scriptContent);
        }
    }
}
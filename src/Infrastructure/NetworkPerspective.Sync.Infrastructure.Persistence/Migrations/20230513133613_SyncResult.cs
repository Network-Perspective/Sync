using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Migrations
{
    public partial class SyncResult : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "InteractionsCount",
                table: "SyncHistory",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<double>(
                name: "SuccessRate",
                table: "SyncHistory",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TasksCount",
                table: "SyncHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InteractionsCount",
                table: "SyncHistory");

            migrationBuilder.DropColumn(
                name: "SuccessRate",
                table: "SyncHistory");

            migrationBuilder.DropColumn(
                name: "TasksCount",
                table: "SyncHistory");
        }
    }
}
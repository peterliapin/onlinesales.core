using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class AddChangeLogTaskLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "change_log_task_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    taskname = table.Column<string>(name: "task_name", type: "text", nullable: false),
                    changelogidmin = table.Column<int>(name: "change_log_id_min", type: "integer", nullable: false),
                    changelogidmax = table.Column<int>(name: "change_log_id_max", type: "integer", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    changesprocessed = table.Column<int>(name: "changes_processed", type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_change_log_task_log", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "change_log_task_log");
        }
    }
}

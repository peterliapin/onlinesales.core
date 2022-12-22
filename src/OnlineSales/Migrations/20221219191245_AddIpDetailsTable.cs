using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class AddIpDetailsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "task_execution_log");

            migrationBuilder.DropColumn(
                name: "created_by_ip",
                table: "task_execution_log");

            migrationBuilder.DropColumn(
                name: "created_by_user_agent",
                table: "task_execution_log");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "task_execution_log");

            migrationBuilder.DropColumn(
                name: "updated_by_ip",
                table: "task_execution_log");

            migrationBuilder.DropColumn(
                name: "updated_by_user_agent",
                table: "task_execution_log");

            migrationBuilder.CreateTable(
                name: "ip_details",
                columns: table => new
                {
                    ip = table.Column<string>(type: "text", nullable: false),
                    continentcode = table.Column<int>(name: "continent_code", type: "integer", nullable: false),
                    countrycode = table.Column<int>(name: "country_code", type: "integer", nullable: false),
                    cityname = table.Column<string>(name: "city_name", type: "text", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ip_details", x => x.ip);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ip_details");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "task_execution_log",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "created_by_ip",
                table: "task_execution_log",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_user_agent",
                table: "task_execution_log",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "task_execution_log",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_ip",
                table: "task_execution_log",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_user_agent",
                table: "task_execution_log",
                type: "text",
                nullable: true);
        }
    }
}

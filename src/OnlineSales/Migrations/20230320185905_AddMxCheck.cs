using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class AddMxCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "account_synced",
                table: "domain");

            migrationBuilder.AddColumn<bool>(
                name: "mx_check",
                table: "domain",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mx_check",
                table: "domain");

            migrationBuilder.AddColumn<bool>(
                name: "account_synced",
                table: "domain",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewFieldsForImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "contact",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "site_url",
                table: "account",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "account",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "site_url",
                table: "account");

            migrationBuilder.DropColumn(
                name: "source",
                table: "account");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainAndAccountImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "favicon_url",
                table: "domain",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "logo_url",
                table: "account",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "favicon_url",
                table: "domain");

            migrationBuilder.DropColumn(
                name: "logo_url",
                table: "account");
        }
    }
}

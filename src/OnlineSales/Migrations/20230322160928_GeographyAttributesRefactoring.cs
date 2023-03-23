using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class GeographyAttributesRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "state_code",
                table: "account",
                newName: "state");

            migrationBuilder.RenameColumn(
                name: "city",
                table: "account",
                newName: "city_name");

            migrationBuilder.AddColumn<string>(
                name: "city_name",
                table: "contact",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "continent_code",
                table: "contact",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "country_code",
                table: "contact",
                type: "integer",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "country_code",
                table: "account");

            migrationBuilder.AddColumn<int>(
                name: "country_code",
                table: "account",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "continent_code",
                table: "account",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "city_name",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "continent_code",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "country_code",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "continent_code",
                table: "account");

            migrationBuilder.RenameColumn(
                name: "state",
                table: "account",
                newName: "state_code");

            migrationBuilder.RenameColumn(
                name: "city_name",
                table: "account",
                newName: "city");

            migrationBuilder.DropColumn(
                name: "country_code",
                table: "account");

            migrationBuilder.AddColumn<string>(
                name: "country_code",
                table: "account",
                type: "text",
                nullable: true);
        }
    }
}

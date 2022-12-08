using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "lang",
                table: "post",
                newName: "language");

            migrationBuilder.AddColumn<string>(
                name: "language",
                table: "email_template",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "language",
                table: "email_group",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "language",
                table: "email_template");

            migrationBuilder.DropColumn(
                name: "language",
                table: "email_group");

            migrationBuilder.RenameColumn(
                name: "language",
                table: "post",
                newName: "lang");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Plugin.Vsto.Migrations
{
    /// <inheritdoc />
    public partial class AddSubfolderColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "subfolder",
                table: "vsto_user_version",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "subfolder",
                table: "vsto_user_version");
        }
    }
}

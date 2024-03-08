using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Plugin.EmailSync.Migrations
{
    /// <inheritdoc />
    public partial class CreatedById : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "imap_account_folder",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "imap_account_folder",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "imap_account",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "imap_account",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "imap_account_folder");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "imap_account_folder");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "imap_account");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "imap_account");
        }
    }
}

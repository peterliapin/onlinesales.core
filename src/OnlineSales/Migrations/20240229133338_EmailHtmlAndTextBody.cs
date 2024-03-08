using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class EmailHtmlAndTextBody : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "body",
                table: "email_log",
                newName: "html_body");

            migrationBuilder.AddColumn<string>(
                name: "text_body",
                table: "email_log",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_email_log_contact_id",
                table: "email_log",
                column: "contact_id");

            migrationBuilder.AddForeignKey(
                name: "fk_email_log_contact_contact_id",
                table: "email_log",
                column: "contact_id",
                principalTable: "contact",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_email_log_contact_contact_id",
                table: "email_log");

            migrationBuilder.DropIndex(
                name: "ix_email_log_contact_id",
                table: "email_log");

            migrationBuilder.DropColumn(
                name: "text_body",
                table: "email_log");

            migrationBuilder.RenameColumn(
                name: "html_body",
                table: "email_log",
                newName: "body");
        }
    }
}

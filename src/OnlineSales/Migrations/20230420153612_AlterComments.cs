using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class AlterComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "approved",
                table: "comment",
                newName: "status");

            migrationBuilder.AddColumn<int>(
                name: "contact_id",
                table: "comment",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "language",
                table: "comment",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_comment_contact_id",
                table: "comment",
                column: "contact_id");

            migrationBuilder.AddForeignKey(
                name: "fk_comment_contact_contact_id",
                table: "comment",
                column: "contact_id",
                principalTable: "contact",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_comment_contact_contact_id",
                table: "comment");

            migrationBuilder.DropIndex(
                name: "ix_comment_contact_id",
                table: "comment");

            migrationBuilder.DropColumn(
                name: "contact_id",
                table: "comment");

            migrationBuilder.DropColumn(
                name: "language",
                table: "comment");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "comment",
                newName: "approved");
        }
    }
}

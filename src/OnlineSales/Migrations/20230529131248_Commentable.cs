using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class Commentable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_comment_content_content_id",
                table: "comment");

            migrationBuilder.DropIndex(
                name: "ix_comment_content_id",
                table: "comment");

            migrationBuilder.RenameColumn(
                name: "content_id",
                table: "comment",
                newName: "commentable_id");

            migrationBuilder.AddColumn<string>(
                name: "commentable_type",
                table: "comment",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE comment SET commentable_type = 'Content'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "commentable_type",
                table: "comment");

            migrationBuilder.RenameColumn(
                name: "commentable_id",
                table: "comment",
                newName: "content_id");

            migrationBuilder.CreateIndex(
                name: "ix_comment_content_id",
                table: "comment",
                column: "content_id");

            migrationBuilder.AddForeignKey(
                name: "fk_comment_content_content_id",
                table: "comment",
                column: "content_id",
                principalTable: "content",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class RelatedEntitiesForInclude : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_email_template_email_group_group_id",
                table: "email_template");

            migrationBuilder.RenameColumn(
                name: "group_id",
                table: "email_template",
                newName: "email_group_id");

            migrationBuilder.RenameIndex(
                name: "ix_email_template_group_id",
                table: "email_template",
                newName: "ix_email_template_email_group_id");

            migrationBuilder.AddForeignKey(
                name: "fk_email_template_email_group_email_group_id",
                table: "email_template",
                column: "email_group_id",
                principalTable: "email_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_email_template_email_group_email_group_id",
                table: "email_template");

            migrationBuilder.RenameColumn(
                name: "email_group_id",
                table: "email_template",
                newName: "group_id");

            migrationBuilder.RenameIndex(
                name: "ix_email_template_email_group_id",
                table: "email_template",
                newName: "ix_email_template_group_id");

            migrationBuilder.AddForeignKey(
                name: "fk_email_template_email_group_group_id",
                table: "email_template",
                column: "group_id",
                principalTable: "email_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

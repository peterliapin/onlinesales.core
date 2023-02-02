using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class AddedAlternateKeyForComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_contact_domain_domain_id",
                table: "contact");

            migrationBuilder.AddColumn<string>(
                name: "key",
                table: "comment",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "fk_contact_domain_domain_id",
                table: "contact",
                column: "domain_id",
                principalTable: "domain",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_contact_domain_domain_id",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "key",
                table: "comment");

            migrationBuilder.AddForeignKey(
                name: "fk_contact_domain_domain_id",
                table: "contact",
                column: "domain_id",
                principalTable: "domain",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

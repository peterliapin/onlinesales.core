using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class DomainIdAddToContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "domain_id",
                table: "contact",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_contact_domain_id",
                table: "contact",
                column: "domain_id");

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

            migrationBuilder.DropIndex(
                name: "ix_contact_domain_id",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "domain_id",
                table: "contact");
        }
    }
}

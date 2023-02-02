using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class DeleteBehaviourContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_contact_domain_domain_id",
                table: "contact");

            migrationBuilder.AddForeignKey(
                name: "fk_contact_domain_domain_id",
                table: "contact",
                column: "domain_id",
                principalTable: "domain",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_contact_domain_domain_id",
                table: "contact");

            migrationBuilder.AddForeignKey(
                name: "fk_contact_domain_domain_id",
                table: "contact",
                column: "domain_id",
                principalTable: "domain",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

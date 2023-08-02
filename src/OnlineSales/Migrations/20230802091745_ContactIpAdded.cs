using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class ContactIpAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "last_ip",
                table: "contact",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_contact_ip_contact_id",
                table: "contact_ip",
                column: "contact_id");

            migrationBuilder.AddForeignKey(
                name: "fk_contact_ip_contact_contact_id",
                table: "contact_ip",
                column: "contact_id",
                principalTable: "contact",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_contact_ip_contact_contact_id",
                table: "contact_ip");

            migrationBuilder.DropIndex(
                name: "ix_contact_ip_contact_id",
                table: "contact_ip");

            migrationBuilder.DropColumn(
                name: "last_ip",
                table: "contact");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class ContactIpTableExt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "created_by_ip",
                table: "contact_ip",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_user_agent",
                table: "contact_ip",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_ip",
                table: "contact_ip",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_user_agent",
                table: "contact_ip",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by_ip",
                table: "contact_ip");

            migrationBuilder.DropColumn(
                name: "created_by_user_agent",
                table: "contact_ip");

            migrationBuilder.DropColumn(
                name: "updated_by_ip",
                table: "contact_ip");

            migrationBuilder.DropColumn(
                name: "updated_by_user_agent",
                table: "contact_ip");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Plugin.SendGrid.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSendGridEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_sendgrid_event_contact_contact_id",
                table: "sendgrid_event");

            migrationBuilder.DropColumn(
                name: "ip",
                table: "sendgrid_event");

            migrationBuilder.AlterColumn<int>(
                name: "contact_id",
                table: "sendgrid_event",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_sendgrid_event_contact_contact_id",
                table: "sendgrid_event",
                column: "contact_id",
                principalTable: "contact",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_sendgrid_event_contact_contact_id",
                table: "sendgrid_event");

            migrationBuilder.AlterColumn<int>(
                name: "contact_id",
                table: "sendgrid_event",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "ip",
                table: "sendgrid_event",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_sendgrid_event_contact_contact_id",
                table: "sendgrid_event",
                column: "contact_id",
                principalTable: "contact",
                principalColumn: "id");
        }
    }
}

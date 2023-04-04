using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Plugin.SendGrid.Migrations
{
    /// <inheritdoc />
    public partial class SendGridEventtablerenaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_sendgrid_events_contact_contact_id",
                table: "sendgrid_events");

            migrationBuilder.DropPrimaryKey(
                name: "pk_sendgrid_events",
                table: "sendgrid_events");

            migrationBuilder.RenameTable(
                name: "sendgrid_events",
                newName: "sendgrid_event");

            migrationBuilder.RenameIndex(
                name: "ix_sendgrid_events_contact_id",
                table: "sendgrid_event",
                newName: "ix_sendgrid_event_contact_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sendgrid_event",
                table: "sendgrid_event",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_sendgrid_event_contact_contact_id",
                table: "sendgrid_event",
                column: "contact_id",
                principalTable: "contact",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_sendgrid_event_contact_contact_id",
                table: "sendgrid_event");

            migrationBuilder.DropPrimaryKey(
                name: "pk_sendgrid_event",
                table: "sendgrid_event");

            migrationBuilder.RenameTable(
                name: "sendgrid_event",
                newName: "sendgrid_events");

            migrationBuilder.RenameIndex(
                name: "ix_sendgrid_event_contact_id",
                table: "sendgrid_events",
                newName: "ix_sendgrid_events_contact_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sendgrid_events",
                table: "sendgrid_events",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_sendgrid_events_contact_contact_id",
                table: "sendgrid_events",
                column: "contact_id",
                principalTable: "contact",
                principalColumn: "id");
        }
    }
}

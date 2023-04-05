using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineSales.Plugin.SendGrid.Migrations
{
    /// <inheritdoc />
    public partial class SendGridEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sendgrid_event",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    @event = table.Column<string>(name: "event", type: "text", nullable: false),
                    message_id = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    ip = table.Column<string>(type: "text", nullable: true),
                    event_id = table.Column<string>(type: "text", nullable: true),
                    contact_id = table.Column<int>(type: "integer", nullable: true),
                    source = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sendgrid_event", x => x.id);
                    table.ForeignKey(
                        name: "fk_sendgrid_event_contact_contact_id",
                        column: x => x.contact_id,
                        principalTable: "contact",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_sendgrid_event_contact_id",
                table: "sendgrid_event",
                column: "contact_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sendgrid_event");
        }
    }
}

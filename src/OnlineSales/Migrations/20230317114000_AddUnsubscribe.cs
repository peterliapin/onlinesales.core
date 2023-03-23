using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class AddUnsubscribe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "unsubscribe_id",
                table: "contact",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "unsubscribe",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    contact_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_ip = table.Column<string>(type: "text", nullable: true),
                    created_by_user_agent = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unsubscribe", x => x.id);
                    table.ForeignKey(
                        name: "fk_unsubscribe_contact_contact_id",
                        column: x => x.contact_id,
                        principalTable: "contact",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_contact_unsubscribe_id",
                table: "contact",
                column: "unsubscribe_id");

            migrationBuilder.CreateIndex(
                name: "ix_unsubscribe_contact_id",
                table: "unsubscribe",
                column: "contact_id");

            migrationBuilder.AddForeignKey(
                name: "fk_contact_unsubscribe_unsubscribe_id",
                table: "contact",
                column: "unsubscribe_id",
                principalTable: "unsubscribe",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_contact_unsubscribe_unsubscribe_id",
                table: "contact");

            migrationBuilder.DropTable(
                name: "unsubscribe");

            migrationBuilder.DropIndex(
                name: "ix_contact_unsubscribe_id",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "unsubscribe_id",
                table: "contact");
        }
    }
}

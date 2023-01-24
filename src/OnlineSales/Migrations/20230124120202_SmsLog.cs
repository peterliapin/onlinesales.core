using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class SmsLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_contact_domain_domain_id",
                table: "contact");

            migrationBuilder.CreateTable(
                name: "sms_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    recipient = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    createdat = table.Column<DateTime>(name: "created_at", type: "timestamp with time zone", nullable: false),
                    createdbyip = table.Column<string>(name: "created_by_ip", type: "text", nullable: true),
                    createdbyuseragent = table.Column<string>(name: "created_by_user_agent", type: "text", nullable: true),
                    updatedat = table.Column<DateTime>(name: "updated_at", type: "timestamp with time zone", nullable: true),
                    updatedbyip = table.Column<string>(name: "updated_by_ip", type: "text", nullable: true),
                    updatedbyuseragent = table.Column<string>(name: "updated_by_user_agent", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sms_log", x => x.id);
                });

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

            migrationBuilder.DropTable(
                name: "sms_log");

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

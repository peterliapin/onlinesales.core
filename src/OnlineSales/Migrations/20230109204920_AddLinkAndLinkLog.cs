using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkAndLinkLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "link",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uid = table.Column<string>(type: "text", nullable: false),
                    destination = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    createdat = table.Column<DateTime>(name: "created_at", type: "timestamp with time zone", nullable: false),
                    createdbyip = table.Column<string>(name: "created_by_ip", type: "text", nullable: true),
                    createdbyuseragent = table.Column<string>(name: "created_by_user_agent", type: "text", nullable: true),
                    updatedat = table.Column<DateTime>(name: "updated_at", type: "timestamp with time zone", nullable: true),
                    updatedbyip = table.Column<string>(name: "updated_by_ip", type: "text", nullable: true),
                    updatedbyuseragent = table.Column<string>(name: "updated_by_user_agent", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_link", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "link_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    linkid = table.Column<int>(name: "link_id", type: "integer", nullable: false),
                    destination = table.Column<string>(type: "text", nullable: false),
                    referrer = table.Column<string>(type: "text", nullable: true),
                    createdat = table.Column<DateTime>(name: "created_at", type: "timestamp with time zone", nullable: false),
                    createdbyip = table.Column<string>(name: "created_by_ip", type: "text", nullable: true),
                    createdbyuseragent = table.Column<string>(name: "created_by_user_agent", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_link_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_link_log_link_link_id",
                        column: x => x.linkid,
                        principalTable: "link",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_customer_email",
                table: "customer",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_link_uid",
                table: "link",
                column: "uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_link_log_link_id",
                table: "link_log",
                column: "link_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "link_log");

            migrationBuilder.DropTable(
                name: "link");

            migrationBuilder.DropIndex(
                name: "ix_customer_email",
                table: "customer");
        }
    }
}

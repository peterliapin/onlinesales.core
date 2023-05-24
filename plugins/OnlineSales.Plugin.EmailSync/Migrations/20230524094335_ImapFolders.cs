using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineSales.Plugin.EmailSync.Migrations
{
    /// <inheritdoc />
    public partial class ImapFolders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_date_time",
                table: "imap_account");

            migrationBuilder.CreateTable(
                name: "imap_account_folder",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    last_uid = table.Column<int>(type: "integer", nullable: false),
                    imap_account_id = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_ip = table.Column<string>(type: "text", nullable: true),
                    created_by_user_agent = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_ip = table.Column<string>(type: "text", nullable: true),
                    updated_by_user_agent = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_imap_account_folder", x => x.id);
                    table.ForeignKey(
                        name: "fk_imap_account_folder_imap_account_imap_account_id",
                        column: x => x.imap_account_id,
                        principalTable: "imap_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_imap_account_host_user_name",
                table: "imap_account",
                columns: new[] { "host", "user_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_imap_account_folder_imap_account_id",
                table: "imap_account_folder",
                column: "imap_account_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "imap_account_folder");

            migrationBuilder.DropIndex(
                name: "ix_imap_account_host_user_name",
                table: "imap_account");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_date_time",
                table: "imap_account",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}

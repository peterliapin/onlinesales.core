using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmailTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_email_log");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "email_template",
                newName: "subject");

            migrationBuilder.RenameColumn(
                name: "template",
                table: "email_template",
                newName: "name");

            migrationBuilder.AddColumn<string>(
                name: "body_template",
                table: "email_template",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "from_email",
                table: "email_template",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "from_name",
                table: "email_template",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "email_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    scheduleid = table.Column<int>(name: "schedule_id", type: "integer", nullable: true),
                    customerid = table.Column<int>(name: "customer_id", type: "integer", nullable: true),
                    templateid = table.Column<int>(name: "template_id", type: "integer", nullable: true),
                    subject = table.Column<string>(type: "text", nullable: false),
                    recipient = table.Column<string>(type: "text", nullable: false),
                    fromemail = table.Column<string>(name: "from_email", type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    createdat = table.Column<DateTime>(name: "created_at", type: "timestamp with time zone", nullable: false),
                    createdbyip = table.Column<string>(name: "created_by_ip", type: "text", nullable: true),
                    createdbyuseragent = table.Column<string>(name: "created_by_user_agent", type: "text", nullable: true),
                    updatedat = table.Column<DateTime>(name: "updated_at", type: "timestamp with time zone", nullable: true),
                    updatedbyip = table.Column<string>(name: "updated_by_ip", type: "text", nullable: true),
                    updatedbyuseragent = table.Column<string>(name: "updated_by_user_agent", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_log", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_log");

            migrationBuilder.DropColumn(
                name: "body_template",
                table: "email_template");

            migrationBuilder.DropColumn(
                name: "from_email",
                table: "email_template");

            migrationBuilder.DropColumn(
                name: "from_name",
                table: "email_template");

            migrationBuilder.RenameColumn(
                name: "subject",
                table: "email_template",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "email_template",
                newName: "template");

            migrationBuilder.CreateTable(
                name: "customer_email_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customeremailscheduleid = table.Column<int>(name: "customer_email_schedule_id", type: "integer", nullable: true),
                    emailtemplateid = table.Column<int>(name: "email_template_id", type: "integer", nullable: true),
                    createdat = table.Column<DateTime>(name: "created_at", type: "timestamp with time zone", nullable: false),
                    createdbyip = table.Column<string>(name: "created_by_ip", type: "text", nullable: true),
                    createdbyuseragent = table.Column<string>(name: "created_by_user_agent", type: "text", nullable: true),
                    scheduleid = table.Column<int>(name: "schedule_id", type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    templateid = table.Column<int>(name: "template_id", type: "integer", nullable: false),
                    updatedat = table.Column<DateTime>(name: "updated_at", type: "timestamp with time zone", nullable: true),
                    updatedbyip = table.Column<string>(name: "updated_by_ip", type: "text", nullable: true),
                    updatedbyuseragent = table.Column<string>(name: "updated_by_user_agent", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_email_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_email_log_customer_email_schedule_customer_email_sch",
                        column: x => x.customeremailscheduleid,
                        principalTable: "customer_email_schedule",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_customer_email_log_email_template_email_template_id",
                        column: x => x.emailtemplateid,
                        principalTable: "email_template",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_customer_email_log_customer_email_schedule_id",
                table: "customer_email_log",
                column: "customer_email_schedule_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_email_log_email_template_id",
                table: "customer_email_log",
                column: "email_template_id");
        }
    }
}

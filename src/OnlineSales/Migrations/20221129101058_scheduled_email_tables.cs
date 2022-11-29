using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class scheduledemailtables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "email_group",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    table.PrimaryKey("pk_email_group", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "email_schedule",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    schedule = table.Column<string>(type: "text", nullable: false),
                    groupid = table.Column<int>(name: "group_id", type: "integer", nullable: false),
                    emailgroupid = table.Column<int>(name: "email_group_id", type: "integer", nullable: true),
                    createdat = table.Column<DateTime>(name: "created_at", type: "timestamp with time zone", nullable: false),
                    createdbyip = table.Column<string>(name: "created_by_ip", type: "text", nullable: true),
                    createdbyuseragent = table.Column<string>(name: "created_by_user_agent", type: "text", nullable: true),
                    updatedat = table.Column<DateTime>(name: "updated_at", type: "timestamp with time zone", nullable: true),
                    updatedbyip = table.Column<string>(name: "updated_by_ip", type: "text", nullable: true),
                    updatedbyuseragent = table.Column<string>(name: "updated_by_user_agent", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_schedule", x => x.id);
                    table.ForeignKey(
                        name: "fk_email_schedule_email_group_email_group_id",
                        column: x => x.emailgroupid,
                        principalTable: "email_group",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "email_template",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    template = table.Column<string>(type: "text", nullable: false),
                    groupid = table.Column<int>(name: "group_id", type: "integer", nullable: false),
                    emailgroupid = table.Column<int>(name: "email_group_id", type: "integer", nullable: true),
                    createdat = table.Column<DateTime>(name: "created_at", type: "timestamp with time zone", nullable: false),
                    createdbyip = table.Column<string>(name: "created_by_ip", type: "text", nullable: true),
                    createdbyuseragent = table.Column<string>(name: "created_by_user_agent", type: "text", nullable: true),
                    updatedat = table.Column<DateTime>(name: "updated_at", type: "timestamp with time zone", nullable: true),
                    updatedbyip = table.Column<string>(name: "updated_by_ip", type: "text", nullable: true),
                    updatedbyuseragent = table.Column<string>(name: "updated_by_user_agent", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_template", x => x.id);
                    table.ForeignKey(
                        name: "fk_email_template_email_group_email_group_id",
                        column: x => x.emailgroupid,
                        principalTable: "email_group",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "customer_email_schedule",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customerid = table.Column<int>(name: "customer_id", type: "integer", nullable: false),
                    scheduleid = table.Column<int>(name: "schedule_id", type: "integer", nullable: false),
                    emailscheduleid = table.Column<int>(name: "email_schedule_id", type: "integer", nullable: true),
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
                    table.PrimaryKey("pk_customer_email_schedule", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_email_schedule_customer_customer_id",
                        column: x => x.customerid,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_customer_email_schedule_email_schedule_email_schedule_id",
                        column: x => x.emailscheduleid,
                        principalTable: "email_schedule",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "customer_email_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    scheduleid = table.Column<int>(name: "schedule_id", type: "integer", nullable: false),
                    customeremailscheduleid = table.Column<int>(name: "customer_email_schedule_id", type: "integer", nullable: true),
                    templateid = table.Column<int>(name: "template_id", type: "integer", nullable: false),
                    emailtemplateid = table.Column<int>(name: "email_template_id", type: "integer", nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "ix_customer_email_schedule_customer_id",
                table: "customer_email_schedule",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_email_schedule_email_schedule_id",
                table: "customer_email_schedule",
                column: "email_schedule_id");

            migrationBuilder.CreateIndex(
                name: "ix_email_schedule_email_group_id",
                table: "email_schedule",
                column: "email_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_email_template_email_group_id",
                table: "email_template",
                column: "email_group_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_email_log");

            migrationBuilder.DropTable(
                name: "customer_email_schedule");

            migrationBuilder.DropTable(
                name: "email_template");

            migrationBuilder.DropTable(
                name: "email_schedule");

            migrationBuilder.DropTable(
                name: "email_group");
        }
    }
}

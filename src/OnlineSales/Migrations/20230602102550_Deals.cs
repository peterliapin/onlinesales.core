using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class Deals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deal_pipeline",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("pk_deal_pipeline", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pipeline_stage",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    deal_pipeline_id = table.Column<int>(type: "integer", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_pipeline_stage", x => x.id);
                    table.ForeignKey(
                        name: "fk_pipeline_stage_deal_pipeline_deal_pipeline_id",
                        column: x => x.deal_pipeline_id,
                        principalTable: "deal_pipeline",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "deal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    deal_pipeline_id = table.Column<int>(type: "integer", nullable: false),
                    pipeline_stage_id = table.Column<int>(type: "integer", nullable: false),
                    deal_money = table.Column<decimal>(type: "numeric", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
                    expected_close_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actual_close_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("pk_deal", x => x.id);
                    table.ForeignKey(
                        name: "fk_deal_account_account_id",
                        column: x => x.account_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_deal_deal_pipeline_deal_pipeline_id",
                        column: x => x.deal_pipeline_id,
                        principalTable: "deal_pipeline",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_deal_pipeline_stage_pipeline_stage_id",
                        column: x => x.pipeline_stage_id,
                        principalTable: "pipeline_stage",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_deal_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contact_deal",
                columns: table => new
                {
                    contacts_id = table.Column<int>(type: "integer", nullable: false),
                    deals_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact_deal", x => new { x.contacts_id, x.deals_id });
                    table.ForeignKey(
                        name: "fk_contact_deal_contact_contacts_id",
                        column: x => x.contacts_id,
                        principalTable: "contact",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_contact_deal_deal_deals_id",
                        column: x => x.deals_id,
                        principalTable: "deal",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_contact_deal_deals_id",
                table: "contact_deal",
                column: "deals_id");

            migrationBuilder.CreateIndex(
                name: "ix_deal_account_id",
                table: "deal",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_deal_deal_pipeline_id",
                table: "deal",
                column: "deal_pipeline_id");

            migrationBuilder.CreateIndex(
                name: "ix_deal_pipeline_stage_id",
                table: "deal",
                column: "pipeline_stage_id");

            migrationBuilder.CreateIndex(
                name: "ix_deal_user_id",
                table: "deal",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_pipeline_stage_deal_pipeline_id",
                table: "pipeline_stage",
                column: "deal_pipeline_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contact_deal");

            migrationBuilder.DropTable(
                name: "deal");

            migrationBuilder.DropTable(
                name: "pipeline_stage");

            migrationBuilder.DropTable(
                name: "deal_pipeline");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class CreatedAndUpdateById : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "unsubscribe",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "promotion",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "promotion",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "order_item",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "order_item",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "order",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "order",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "link_log",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "link",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "link",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "file",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "file",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "email_template",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "email_template",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "email_schedule",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "email_schedule",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "email_log",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "email_log",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "email_group",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "email_group",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "discount",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "discount",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "deal_pipeline_stage",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "deal_pipeline_stage",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "deal_pipeline",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "deal_pipeline",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "deal",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "deal",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "content",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "content",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "contact_email_schedule",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "contact_email_schedule",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "contact",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "contact",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "comment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "comment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "account",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by_id",
                table: "account",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "unsubscribe");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "promotion");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "promotion");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "order");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "order");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "media");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "media");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "link_log");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "link");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "link");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "file");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "file");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "email_template");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "email_template");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "email_schedule");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "email_schedule");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "email_log");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "email_log");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "email_group");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "email_group");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "discount");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "discount");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "deal_pipeline_stage");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "deal_pipeline_stage");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "deal_pipeline");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "deal_pipeline");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "deal");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "deal");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "content");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "content");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "contact_email_schedule");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "contact_email_schedule");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "comment");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "comment");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "account");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "account");
        }
    }
}

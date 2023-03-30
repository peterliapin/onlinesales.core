using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceToAllEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "source",
                table: "unsubscribe",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "task_execution_log",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "order_item",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "order",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "link_log",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "link",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "email_template",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "email_schedule",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "email_log",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "email_group",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "source",
                table: "domain",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "content",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "contact_email_schedule",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "comment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "change_log_task_log",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "change_log",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source",
                table: "task_execution_log");

            migrationBuilder.DropColumn(
                name: "source",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "source",
                table: "order");

            migrationBuilder.DropColumn(
                name: "source",
                table: "media");

            migrationBuilder.DropColumn(
                name: "source",
                table: "link_log");

            migrationBuilder.DropColumn(
                name: "source",
                table: "link");

            migrationBuilder.DropColumn(
                name: "source",
                table: "email_template");

            migrationBuilder.DropColumn(
                name: "source",
                table: "email_schedule");

            migrationBuilder.DropColumn(
                name: "source",
                table: "email_log");

            migrationBuilder.DropColumn(
                name: "source",
                table: "email_group");

            migrationBuilder.DropColumn(
                name: "source",
                table: "content");

            migrationBuilder.DropColumn(
                name: "source",
                table: "contact_email_schedule");

            migrationBuilder.DropColumn(
                name: "source",
                table: "comment");

            migrationBuilder.DropColumn(
                name: "source",
                table: "change_log_task_log");

            migrationBuilder.DropColumn(
                name: "source",
                table: "change_log");

            migrationBuilder.AlterColumn<string>(
                name: "source",
                table: "unsubscribe",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "source",
                table: "domain",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}

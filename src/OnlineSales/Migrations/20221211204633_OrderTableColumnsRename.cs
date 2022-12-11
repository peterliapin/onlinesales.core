using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class OrderTableColumnsRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_email_template_email_group_email_group_id",
                table: "email_template");

            migrationBuilder.DropIndex(
                name: "ix_email_template_email_group_id",
                table: "email_template");

            migrationBuilder.DropColumn(
                name: "email_group_id",
                table: "email_template");

            migrationBuilder.AddColumn<decimal>(
                name: "unit_price",
                table: "order_item",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "exchange_rate",
                table: "order",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "ix_email_template_group_id",
                table: "email_template",
                column: "group_id");

            migrationBuilder.AddForeignKey(
                name: "fk_email_template_email_group_group_id",
                table: "email_template",
                column: "group_id",
                principalTable: "email_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_email_template_email_group_group_id",
                table: "email_template");

            migrationBuilder.DropIndex(
                name: "ix_email_template_group_id",
                table: "email_template");

            migrationBuilder.DropColumn(
                name: "unit_price",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "exchange_rate",
                table: "order");

            migrationBuilder.AddColumn<int>(
                name: "email_group_id",
                table: "email_template",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_email_template_email_group_id",
                table: "email_template",
                column: "email_group_id");

            migrationBuilder.AddForeignKey(
                name: "fk_email_template_email_group_email_group_id",
                table: "email_template",
                column: "email_group_id",
                principalTable: "email_group",
                principalColumn: "id");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class OrderIdInDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_order_item_discount_discount_id",
                table: "order_item");

            migrationBuilder.DropIndex(
                name: "ix_order_item_discount_id",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "discount_id",
                table: "order_item");

            migrationBuilder.RenameColumn(
                name: "promotion_code",
                table: "promotion",
                newName: "code");

            migrationBuilder.RenameIndex(
                name: "ix_promotion_promotion_code",
                table: "promotion",
                newName: "ix_promotion_code");

            migrationBuilder.AlterColumn<DateTime>(
                name: "start_date",
                table: "promotion",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "end_date",
                table: "promotion",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<decimal>(
                name: "items_currency_total",
                table: "order",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "order_item_id",
                table: "discount",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_discount_order_item_id",
                table: "discount",
                column: "order_item_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_discount_order_item_order_item_id",
                table: "discount",
                column: "order_item_id",
                principalTable: "order_item",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_discount_order_item_order_item_id",
                table: "discount");

            migrationBuilder.DropIndex(
                name: "ix_discount_order_item_id",
                table: "discount");

            migrationBuilder.DropColumn(
                name: "items_currency_total",
                table: "order");

            migrationBuilder.DropColumn(
                name: "order_item_id",
                table: "discount");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "promotion",
                newName: "promotion_code");

            migrationBuilder.RenameIndex(
                name: "ix_promotion_code",
                table: "promotion",
                newName: "ix_promotion_promotion_code");

            migrationBuilder.AlterColumn<DateTime>(
                name: "start_date",
                table: "promotion",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "end_date",
                table: "promotion",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "discount_id",
                table: "order_item",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_item_discount_id",
                table: "order_item",
                column: "discount_id");

            migrationBuilder.AddForeignKey(
                name: "fk_order_item_discount_discount_id",
                table: "order_item",
                column: "discount_id",
                principalTable: "discount",
                principalColumn: "id");
        }
    }
}

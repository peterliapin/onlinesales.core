using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class OrdersReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "license_code",
                table: "order_item");

            migrationBuilder.AddColumn<string>(
                name: "ref_no",
                table: "order_item",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "commission",
                table: "order",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ref_no",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "commission",
                table: "order");

            migrationBuilder.AddColumn<string>(
                name: "license_code",
                table: "order_item",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}

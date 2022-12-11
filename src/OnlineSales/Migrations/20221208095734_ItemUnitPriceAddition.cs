using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class ItemUnitPriceAddition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "unit_price",
                table: "order_item",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "unit_price",
                table: "order_item");
        }
    }
}

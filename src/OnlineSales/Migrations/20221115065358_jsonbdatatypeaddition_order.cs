using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class jsonbdatatypeadditionorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "data",
                table: "orders",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "data",
                table: "orders");
        }
    }
}

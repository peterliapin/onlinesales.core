using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class RenameImagesTableToImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_images",
                table: "images");

            migrationBuilder.RenameTable(
                name: "images",
                newName: "image");

            migrationBuilder.AddPrimaryKey(
                name: "pk_image",
                table: "image",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_image",
                table: "image");

            migrationBuilder.RenameTable(
                name: "image",
                newName: "images");

            migrationBuilder.AddPrimaryKey(
                name: "pk_images",
                table: "images",
                column: "id");
        }
    }
}

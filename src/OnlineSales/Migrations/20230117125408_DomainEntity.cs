using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class DomainEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "shared",
                table: "domain");

            migrationBuilder.AlterColumn<bool>(
                name: "disposable",
                table: "domain",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<bool>(
                name: "catch_all",
                table: "domain",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "domain",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "dns_check",
                table: "domain",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dns_records",
                table: "domain",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "free",
                table: "domain",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "http_check",
                table: "domain",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "domain",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "url",
                table: "domain",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_ref_no",
                table: "order",
                column: "ref_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ip_details_ip",
                table: "ip_details",
                column: "ip",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_order_ref_no",
                table: "order");

            migrationBuilder.DropIndex(
                name: "ix_ip_details_ip",
                table: "ip_details");

            migrationBuilder.DropColumn(
                name: "catch_all",
                table: "domain");

            migrationBuilder.DropColumn(
                name: "description",
                table: "domain");

            migrationBuilder.DropColumn(
                name: "dns_check",
                table: "domain");

            migrationBuilder.DropColumn(
                name: "dns_records",
                table: "domain");

            migrationBuilder.DropColumn(
                name: "free",
                table: "domain");

            migrationBuilder.DropColumn(
                name: "http_check",
                table: "domain");

            migrationBuilder.DropColumn(
                name: "title",
                table: "domain");

            migrationBuilder.DropColumn(
                name: "url",
                table: "domain");

            migrationBuilder.AlterColumn<bool>(
                name: "disposable",
                table: "domain",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "shared",
                table: "domain",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}

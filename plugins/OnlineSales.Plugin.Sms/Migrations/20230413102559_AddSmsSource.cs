using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Plugin.Sms.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "sms_log",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source",
                table: "sms_log");
        }
    }
}

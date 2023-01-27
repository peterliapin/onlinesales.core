using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class AccountImprovemts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_account_domain_domain_id",
                table: "account");

            migrationBuilder.AlterColumn<string[]>(
                name: "tags",
                table: "account",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string[]),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<int>(
                name: "domain_id",
                table: "account",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "ix_account_name",
                table: "account",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_account_domain_domain_id",
                table: "account",
                column: "domain_id",
                principalTable: "domain",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_account_domain_domain_id",
                table: "account");

            migrationBuilder.DropIndex(
                name: "ix_account_name",
                table: "account");

            migrationBuilder.AlterColumn<string[]>(
                name: "tags",
                table: "account",
                type: "jsonb",
                nullable: false,
                defaultValue: new string[0],
                oldClrType: typeof(string[]),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "domain_id",
                table: "account",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_account_domain_domain_id",
                table: "account",
                column: "domain_id",
                principalTable: "domain",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

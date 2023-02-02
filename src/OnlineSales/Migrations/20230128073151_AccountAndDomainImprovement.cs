using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class AccountAndDomainImprovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_account_domain_domain_id",
                table: "account");

            migrationBuilder.DropIndex(
                name: "ix_account_domain_id",
                table: "account");

            migrationBuilder.DropColumn(
                name: "domain_id",
                table: "account");

            migrationBuilder.RenameColumn(
                name: "employees_rate",
                table: "account",
                newName: "employees_range");

            migrationBuilder.RenameColumn(
                name: "country",
                table: "account",
                newName: "country_code");

            migrationBuilder.AddColumn<int>(
                name: "account_id",
                table: "domain",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "account_synced",
                table: "domain",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "account_id",
                table: "contact",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string[]>(
                name: "tags",
                table: "account",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string[]),
                oldType: "jsonb");

            migrationBuilder.CreateIndex(
                name: "ix_domain_account_id",
                table: "domain",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_contact_account_id",
                table: "contact",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_account_name",
                table: "account",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_contact_account_account_id",
                table: "contact",
                column: "account_id",
                principalTable: "account",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_domain_account_account_id",
                table: "domain",
                column: "account_id",
                principalTable: "account",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_contact_account_account_id",
                table: "contact");

            migrationBuilder.DropForeignKey(
                name: "fk_domain_account_account_id",
                table: "domain");

            migrationBuilder.DropIndex(
                name: "ix_domain_account_id",
                table: "domain");

            migrationBuilder.DropIndex(
                name: "ix_contact_account_id",
                table: "contact");

            migrationBuilder.DropIndex(
                name: "ix_account_name",
                table: "account");

            migrationBuilder.DropColumn(
                name: "account_id",
                table: "domain");

            migrationBuilder.DropColumn(
                name: "account_synced",
                table: "domain");

            migrationBuilder.DropColumn(
                name: "account_id",
                table: "contact");

            migrationBuilder.RenameColumn(
                name: "employees_range",
                table: "account",
                newName: "employees_rate");

            migrationBuilder.RenameColumn(
                name: "country_code",
                table: "account",
                newName: "country");

            migrationBuilder.AlterColumn<string[]>(
                name: "tags",
                table: "account",
                type: "jsonb",
                nullable: false,
                defaultValue: new string[0],
                oldClrType: typeof(string[]),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "domain_id",
                table: "account",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_account_domain_id",
                table: "account",
                column: "domain_id");

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

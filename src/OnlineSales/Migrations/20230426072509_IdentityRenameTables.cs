using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class IdentityRenameTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "roles");

            migrationBuilder.RenameTable(
                name: "UserTokens",
                newName: "user_tokens");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                newName: "user_roles");

            migrationBuilder.RenameTable(
                name: "UserLogins",
                newName: "user_logins");

            migrationBuilder.RenameTable(
                name: "UserClaims",
                newName: "user_claims");

            migrationBuilder.RenameTable(
                name: "RoleClaims",
                newName: "role_claims");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "roles",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "user_tokens",
                newName: "UserTokens");

            migrationBuilder.RenameTable(
                name: "user_roles",
                newName: "UserRoles");

            migrationBuilder.RenameTable(
                name: "user_logins",
                newName: "UserLogins");

            migrationBuilder.RenameTable(
                name: "user_claims",
                newName: "UserClaims");

            migrationBuilder.RenameTable(
                name: "role_claims",
                newName: "RoleClaims");
        }
    }
}

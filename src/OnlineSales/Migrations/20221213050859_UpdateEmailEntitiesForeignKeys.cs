using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmailEntitiesForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_customer_email_schedule_email_schedule_email_schedule_id",
                table: "customer_email_schedule");

            migrationBuilder.DropForeignKey(
                name: "fk_email_schedule_email_group_email_group_id",
                table: "email_schedule");

            migrationBuilder.DropIndex(
                name: "ix_email_schedule_email_group_id",
                table: "email_schedule");

            migrationBuilder.DropIndex(
                name: "ix_customer_email_schedule_email_schedule_id",
                table: "customer_email_schedule");

            migrationBuilder.DropColumn(
                name: "email_group_id",
                table: "email_schedule");
             
            migrationBuilder.DropColumn(
                name: "email_schedule_id",
                table: "customer_email_schedule");

            migrationBuilder.CreateIndex(
                name: "ix_email_schedule_group_id",
                table: "email_schedule",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_email_schedule_schedule_id",
                table: "customer_email_schedule",
                column: "schedule_id");

            migrationBuilder.AddForeignKey(
                name: "fk_customer_email_schedule_email_schedule_schedule_id",
                table: "customer_email_schedule",
                column: "schedule_id",
                principalTable: "email_schedule",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_email_schedule_email_group_group_id",
                table: "email_schedule",
                column: "group_id",
                principalTable: "email_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_customer_email_schedule_email_schedule_schedule_id",
                table: "customer_email_schedule");

            migrationBuilder.DropForeignKey(
                name: "fk_email_schedule_email_group_group_id",
                table: "email_schedule");

            migrationBuilder.DropIndex(
                name: "ix_email_schedule_group_id",
                table: "email_schedule");

            migrationBuilder.DropIndex(
                name: "ix_customer_email_schedule_schedule_id",
                table: "customer_email_schedule");

            migrationBuilder.AddColumn<int>(
                name: "email_group_id",
                table: "email_schedule",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "email_schedule_id",
                table: "customer_email_schedule",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_email_schedule_email_group_id",
                table: "email_schedule",
                column: "email_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_email_schedule_email_schedule_id",
                table: "customer_email_schedule",
                column: "email_schedule_id");

            migrationBuilder.AddForeignKey(
                name: "fk_customer_email_schedule_email_schedule_email_schedule_id",
                table: "customer_email_schedule",
                column: "email_schedule_id",
                principalTable: "email_schedule",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_email_schedule_email_group_email_group_id",
                table: "email_schedule",
                column: "email_group_id",
                principalTable: "email_group",
                principalColumn: "id");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_customer",
                table: "customer");

            migrationBuilder.RenameTable(
                name: "customer",
                newName: "customers");

            migrationBuilder.AddPrimaryKey(
                name: "pk_customers",
                table: "customers",
                column: "id");

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customerid = table.Column<int>(name: "customer_id", type: "integer", nullable: false),
                    customerip = table.Column<string>(name: "customer_ip", type: "text", nullable: true),
                    refno = table.Column<string>(name: "ref_no", type: "text", nullable: false),
                    ordernumber = table.Column<string>(name: "order_number", type: "text", nullable: true),
                    total = table.Column<decimal>(type: "numeric", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
                    currencytotal = table.Column<decimal>(name: "currency_total", type: "numeric", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    affiliatename = table.Column<string>(name: "affiliate_name", type: "text", nullable: true),
                    testorder = table.Column<bool>(name: "test_order", type: "boolean", nullable: false),
                    createdat = table.Column<DateTime>(name: "created_at", type: "timestamp with time zone", nullable: false),
                    createdbyip = table.Column<string>(name: "created_by_ip", type: "text", nullable: true),
                    createdbyuseragent = table.Column<string>(name: "created_by_user_agent", type: "text", nullable: true),
                    updatedat = table.Column<DateTime>(name: "updated_at", type: "timestamp with time zone", nullable: true),
                    updatedbyip = table.Column<string>(name: "updated_by_ip", type: "text", nullable: true),
                    updatedbyuseragent = table.Column<string>(name: "updated_by_user_agent", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_orders_customers_customer_id",
                        column: x => x.customerid,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    orderid = table.Column<int>(name: "order_id", type: "integer", nullable: false),
                    productname = table.Column<string>(name: "product_name", type: "text", nullable: false),
                    licensecode = table.Column<string>(name: "license_code", type: "text", nullable: false),
                    total = table.Column<decimal>(type: "numeric", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
                    currencytotal = table.Column<decimal>(name: "currency_total", type: "numeric", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    createdat = table.Column<DateTime>(name: "created_at", type: "timestamp with time zone", nullable: false),
                    createdbyip = table.Column<string>(name: "created_by_ip", type: "text", nullable: true),
                    createdbyuseragent = table.Column<string>(name: "created_by_user_agent", type: "text", nullable: true),
                    updatedat = table.Column<DateTime>(name: "updated_at", type: "timestamp with time zone", nullable: true),
                    updatedbyip = table.Column<string>(name: "updated_by_ip", type: "text", nullable: true),
                    updatedbyuseragent = table.Column<string>(name: "updated_by_user_agent", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_items_orders_order_id",
                        column: x => x.orderid,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_customer_id",
                table: "orders",
                column: "customer_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_customers",
                table: "customers");

            migrationBuilder.RenameTable(
                name: "customers",
                newName: "customer");

            migrationBuilder.AddPrimaryKey(
                name: "pk_customer",
                table: "customer",
                column: "id");
        }
    }
}

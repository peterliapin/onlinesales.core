using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class singulartablenames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_comments_comments_parent_id",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "fk_comments_posts_post_id",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "fk_order_items_orders_order_id",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "fk_orders_customers_customer_id",
                table: "orders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_task_execution_logs",
                table: "task_execution_logs");

            migrationBuilder.DropPrimaryKey(
                name: "pk_posts",
                table: "posts");

            migrationBuilder.DropPrimaryKey(
                name: "pk_orders",
                table: "orders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_order_items",
                table: "order_items");

            migrationBuilder.DropPrimaryKey(
                name: "pk_customers",
                table: "customers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_comments",
                table: "comments");

            migrationBuilder.RenameTable(
                name: "task_execution_logs",
                newName: "task_execution_log");

            migrationBuilder.RenameTable(
                name: "posts",
                newName: "post");

            migrationBuilder.RenameTable(
                name: "orders",
                newName: "order");

            migrationBuilder.RenameTable(
                name: "order_items",
                newName: "order_item");

            migrationBuilder.RenameTable(
                name: "customers",
                newName: "customer");

            migrationBuilder.RenameTable(
                name: "comments",
                newName: "comment");

            migrationBuilder.RenameIndex(
                name: "ix_orders_customer_id",
                table: "order",
                newName: "ix_order_customer_id");

            migrationBuilder.RenameIndex(
                name: "ix_order_items_order_id",
                table: "order_item",
                newName: "ix_order_item_order_id");

            migrationBuilder.RenameIndex(
                name: "ix_comments_post_id",
                table: "comment",
                newName: "ix_comment_post_id");

            migrationBuilder.RenameIndex(
                name: "ix_comments_parent_id",
                table: "comment",
                newName: "ix_comment_parent_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_task_execution_log",
                table: "task_execution_log",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_post",
                table: "post",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_order",
                table: "order",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_order_item",
                table: "order_item",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_customer",
                table: "customer",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_comment",
                table: "comment",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_comment_comment_parent_id",
                table: "comment",
                column: "parent_id",
                principalTable: "comment",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_comment_post_post_id",
                table: "comment",
                column: "post_id",
                principalTable: "post",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_order_customer_customer_id",
                table: "order",
                column: "customer_id",
                principalTable: "customer",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_order_item_order_order_id",
                table: "order_item",
                column: "order_id",
                principalTable: "order",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_comment_comment_parent_id",
                table: "comment");

            migrationBuilder.DropForeignKey(
                name: "fk_comment_post_post_id",
                table: "comment");

            migrationBuilder.DropForeignKey(
                name: "fk_order_customer_customer_id",
                table: "order");

            migrationBuilder.DropForeignKey(
                name: "fk_order_item_order_order_id",
                table: "order_item");

            migrationBuilder.DropPrimaryKey(
                name: "pk_task_execution_log",
                table: "task_execution_log");

            migrationBuilder.DropPrimaryKey(
                name: "pk_post",
                table: "post");

            migrationBuilder.DropPrimaryKey(
                name: "pk_order_item",
                table: "order_item");

            migrationBuilder.DropPrimaryKey(
                name: "pk_order",
                table: "order");

            migrationBuilder.DropPrimaryKey(
                name: "pk_customer",
                table: "customer");

            migrationBuilder.DropPrimaryKey(
                name: "pk_comment",
                table: "comment");

            migrationBuilder.RenameTable(
                name: "task_execution_log",
                newName: "task_execution_logs");

            migrationBuilder.RenameTable(
                name: "post",
                newName: "posts");

            migrationBuilder.RenameTable(
                name: "order_item",
                newName: "order_items");

            migrationBuilder.RenameTable(
                name: "order",
                newName: "orders");

            migrationBuilder.RenameTable(
                name: "customer",
                newName: "customers");

            migrationBuilder.RenameTable(
                name: "comment",
                newName: "comments");

            migrationBuilder.RenameIndex(
                name: "ix_order_item_order_id",
                table: "order_items",
                newName: "ix_order_items_order_id");

            migrationBuilder.RenameIndex(
                name: "ix_order_customer_id",
                table: "orders",
                newName: "ix_orders_customer_id");

            migrationBuilder.RenameIndex(
                name: "ix_comment_post_id",
                table: "comments",
                newName: "ix_comments_post_id");

            migrationBuilder.RenameIndex(
                name: "ix_comment_parent_id",
                table: "comments",
                newName: "ix_comments_parent_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_task_execution_logs",
                table: "task_execution_logs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_posts",
                table: "posts",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_order_items",
                table: "order_items",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_orders",
                table: "orders",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_customers",
                table: "customers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_comments",
                table: "comments",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_comments_comments_parent_id",
                table: "comments",
                column: "parent_id",
                principalTable: "comments",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_comments_posts_post_id",
                table: "comments",
                column: "post_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_order_items_orders_order_id",
                table: "order_items",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_orders_customers_customer_id",
                table: "orders",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

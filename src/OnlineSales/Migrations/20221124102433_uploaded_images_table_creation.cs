using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class uploadedimagestablecreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "uploaded_images",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    scopeid = table.Column<string>(name: "scope_id", type: "text", nullable: false),
                    filename = table.Column<string>(name: "file_name", type: "text", nullable: false),
                    filesize = table.Column<long>(name: "file_size", type: "bigint", nullable: false),
                    returnedfilename = table.Column<string>(name: "returned_file_name", type: "text", nullable: false),
                    mimetype = table.Column<string>(name: "mime_type", type: "text", nullable: false),
                    imagebinarydata = table.Column<byte[]>(name: "image_binary_data", type: "bytea", nullable: false),
                    createdat = table.Column<DateTime>(name: "created_at", type: "timestamp with time zone", nullable: false),
                    createdbyip = table.Column<string>(name: "created_by_ip", type: "text", nullable: true),
                    createdbyuseragent = table.Column<string>(name: "created_by_user_agent", type: "text", nullable: true),
                    updatedat = table.Column<DateTime>(name: "updated_at", type: "timestamp with time zone", nullable: true),
                    updatedbyip = table.Column<string>(name: "updated_by_ip", type: "text", nullable: true),
                    updatedbyuseragent = table.Column<string>(name: "updated_by_user_agent", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_uploaded_images", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "uploaded_images");
        }
    }
}

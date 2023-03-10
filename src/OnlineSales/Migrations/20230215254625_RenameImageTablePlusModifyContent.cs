using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class RenameImageTablePlusModifyContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "image", newName: "media");
            migrationBuilder.Sql("UPDATE  post SET  cover_image_url = regexp_replace( cover_image_url, '/api/images/(.*)/(.*)', '/api/media/\\1/\\2', 'g')");
            migrationBuilder.Sql("UPDATE  post SET  content = regexp_replace( content, '/api/images/(.*)/(.*)', '/api/media/\\1/\\2', 'gin')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "media", newName: "image");
            migrationBuilder.Sql("UPDATE  post SET  cover_image_url = regexp_replace( cover_image_url, '/api/media/(.*)/(.*)', '/api/images/\\1/\\2', 'g')");
            migrationBuilder.Sql("UPDATE  post SET  content = regexp_replace( content, '/api/media/(.*)/(.*)', '/api/images/\\1/\\2', 'gin')");
        }
    }
}

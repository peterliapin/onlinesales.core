using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class ExtendContactAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "birthday",
                table: "contact",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_name",
                table: "contact",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "department",
                table: "contact",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "job_title",
                table: "contact",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "middle_name",
                table: "contact",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "prefix",
                table: "contact",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Dictionary<string, string>>(
                name: "social_media",
                table: "contact",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "birthday",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "company_name",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "department",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "job_title",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "middle_name",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "prefix",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "social_media",
                table: "contact");
        }
    }
}

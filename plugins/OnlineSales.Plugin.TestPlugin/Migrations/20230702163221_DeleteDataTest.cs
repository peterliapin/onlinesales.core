﻿// <auto-generated />
using Microsoft.EntityFrameworkCore.Migrations;
using OnlineSales.Plugin.TestPlugin.TestData;

#nullable disable

namespace OnlineSales.Plugin.TestPlugin.Migrations
{
    /// <inheritdoc />
    public partial class DeleteDataTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            for (int i = 1; i <= ChangeLogMigrationsTestData.NumberOfDeletedEntities; i++)
            {
                migrationBuilder.DeleteData(
                    keyColumn: "id",
                    keyValue: i,
                    table: "test_entity");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not needed
        }
    }
}

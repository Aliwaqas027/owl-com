﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Warehouses.Migrations
{
    public partial class WarehouseDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "warehouses",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "warehouses");
        }
    }
}

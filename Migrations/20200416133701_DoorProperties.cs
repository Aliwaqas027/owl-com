using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Warehouses.Migrations
{
    public partial class DoorProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "permissions");

            migrationBuilder.AddColumn<string>(
                name: "Properties",
                table: "doors",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Properties",
                table: "doors");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "permissions",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}

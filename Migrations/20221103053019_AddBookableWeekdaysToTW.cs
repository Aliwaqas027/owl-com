using Microsoft.EntityFrameworkCore.Migrations;
using System.Collections.Generic;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AddBookableWeekdaysToTW : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookableWeekdays",
                table: "availabilities");

            migrationBuilder.AddColumn<List<int>>(
                name: "BookableWeekdays",
                table: "time_windows",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookableWeekdays",
                table: "time_windows");

            migrationBuilder.AddColumn<List<int>>(
                name: "BookableWeekdays",
                table: "availabilities",
                type: "jsonb",
                nullable: true);
        }
    }
}

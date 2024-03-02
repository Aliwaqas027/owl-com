using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AddDoorDailyPalletsLimit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyPalletsLimit",
                table: "doors",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyPalletsLimit",
                table: "doors");
        }
    }
}

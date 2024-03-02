using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AddHideFieldForCarriers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FixedTimeWindowType",
                table: "availabilities");

            migrationBuilder.AddColumn<bool>(
                name: "HideForCarriers",
                table: "reservation_fields",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HideForCarriers",
                table: "reservation_fields");

            migrationBuilder.AddColumn<int>(
                name: "FixedTimeWindowType",
                table: "availabilities",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

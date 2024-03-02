using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AddValuesArrayToFieldsFilter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "Values",
                table: "TimeWindowFieldsFilters",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "Values",
                table: "door_fields_filter",
                type: "text[]",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Values",
                table: "TimeWindowFieldsFilters");

            migrationBuilder.DropColumn(
                name: "Values",
                table: "door_fields_filter");
        }
    }
}

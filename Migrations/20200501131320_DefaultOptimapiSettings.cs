using Microsoft.EntityFrameworkCore.Migrations;

namespace Warehouses.Migrations
{
    public partial class DefaultOptimapiSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StopsSettings",
                table: "optimapi_servers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehiclesSettings",
                table: "optimapi_servers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StopsSettings",
                table: "optimapi_servers");

            migrationBuilder.DropColumn(
                name: "VehiclesSettings",
                table: "optimapi_servers");
        }
    }
}

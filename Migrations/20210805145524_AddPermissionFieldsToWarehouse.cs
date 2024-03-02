using Microsoft.EntityFrameworkCore.Migrations;

namespace Warehouses.Migrations
{
    public partial class AddPermissionFieldsToWarehouse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "canCarrierDeleteReservation",
                table: "warehouses",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "canCarrierEditReservation",
                table: "warehouses",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "canCarrierDeleteReservation",
                table: "warehouses");

            migrationBuilder.DropColumn(
                name: "canCarrierEditReservation",
                table: "warehouses");
        }
    }
}

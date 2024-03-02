using Microsoft.EntityFrameworkCore.Migrations;

namespace Warehouses.Migrations
{
    public partial class RenameUserWarehouseConfirmed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WarehouseConfirmed",
                table: "users",
                newName: "Confirmed");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
               name: "Confirmed",
               table: "users",
               newName: "WarehouseConfirmed");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class WarehouseImportantFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImportantFieldWarehouseId",
                table: "reservation_fields",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservation_fields_ImportantFieldWarehouseId",
                table: "reservation_fields",
                column: "ImportantFieldWarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_reservation_fields_warehouses_ImportantFieldWarehouseId",
                table: "reservation_fields",
                column: "ImportantFieldWarehouseId",
                principalTable: "warehouses",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reservation_fields_warehouses_ImportantFieldWarehouseId",
                table: "reservation_fields");

            migrationBuilder.DropIndex(
                name: "IX_reservation_fields_ImportantFieldWarehouseId",
                table: "reservation_fields");

            migrationBuilder.DropColumn(
                name: "ImportantFieldWarehouseId",
                table: "reservation_fields");
        }
    }
}

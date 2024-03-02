using Microsoft.EntityFrameworkCore.Migrations;

namespace Warehouses.Migrations
{
    public partial class TwoPhaseReservation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reservations_doors_DoorId",
                table: "reservations");

            migrationBuilder.AlterColumn<int>(
                name: "DoorId",
                table: "reservations",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "reservations",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservations_WarehouseId",
                table: "reservations",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_reservations_doors_DoorId",
                table: "reservations",
                column: "DoorId",
                principalTable: "doors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_reservations_warehouses_WarehouseId",
                table: "reservations",
                column: "WarehouseId",
                principalTable: "warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reservations_doors_DoorId",
                table: "reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_reservations_warehouses_WarehouseId",
                table: "reservations");

            migrationBuilder.DropIndex(
                name: "IX_reservations_WarehouseId",
                table: "reservations");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "reservations");

            migrationBuilder.AlterColumn<int>(
                name: "DoorId",
                table: "reservations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_reservations_doors_DoorId",
                table: "reservations",
                column: "DoorId",
                principalTable: "doors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

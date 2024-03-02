using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AddTimeWindowToReservation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FixedTimeWindowId",
                table: "reservations",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservations_FixedTimeWindowId",
                table: "reservations",
                column: "FixedTimeWindowId");

            migrationBuilder.AddForeignKey(
                name: "FK_reservations_time_windows_FixedTimeWindowId",
                table: "reservations",
                column: "FixedTimeWindowId",
                principalTable: "time_windows",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reservations_time_windows_FixedTimeWindowId",
                table: "reservations");

            migrationBuilder.DropIndex(
                name: "IX_reservations_FixedTimeWindowId",
                table: "reservations");

            migrationBuilder.DropColumn(
                name: "FixedTimeWindowId",
                table: "reservations");
        }
    }
}

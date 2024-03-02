using Microsoft.EntityFrameworkCore.Migrations;

namespace Warehouses.Migrations
{
    public partial class WarehouseImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_files_reservations_ReservationId",
                table: "files");

            migrationBuilder.AlterColumn<int>(
                name: "ReservationId",
                table: "files",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "files",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_files_UserId",
                table: "files",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_files_reservations_ReservationId",
                table: "files",
                column: "ReservationId",
                principalTable: "reservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_files_users_UserId",
                table: "files",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_files_reservations_ReservationId",
                table: "files");

            migrationBuilder.DropForeignKey(
                name: "FK_files_users_UserId",
                table: "files");

            migrationBuilder.DropIndex(
                name: "IX_files_UserId",
                table: "files");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "files");

            migrationBuilder.AlterColumn<int>(
                name: "ReservationId",
                table: "files",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_files_reservations_ReservationId",
                table: "files",
                column: "ReservationId",
                principalTable: "reservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AllowAnonymousReservations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_recurring_reservations_users_CarrierId",
                table: "recurring_reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_reservations_users_CarrierId",
                table: "reservations");

            migrationBuilder.AlterColumn<int>(
                name: "CarrierId",
                table: "reservations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "CarrierId",
                table: "recurring_reservations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_recurring_reservations_users_CarrierId",
                table: "recurring_reservations",
                column: "CarrierId",
                principalTable: "users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_reservations_users_CarrierId",
                table: "reservations",
                column: "CarrierId",
                principalTable: "users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_recurring_reservations_users_CarrierId",
                table: "recurring_reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_reservations_users_CarrierId",
                table: "reservations");

            migrationBuilder.AlterColumn<int>(
                name: "CarrierId",
                table: "reservations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CarrierId",
                table: "recurring_reservations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_recurring_reservations_users_CarrierId",
                table: "recurring_reservations",
                column: "CarrierId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reservations_users_CarrierId",
                table: "reservations",
                column: "CarrierId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

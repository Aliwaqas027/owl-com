using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class CompanyReservationFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reservation_fields_users_UserId",
                table: "reservation_fields");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "reservation_fields",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_reservation_fields_UserId",
                table: "reservation_fields",
                newName: "IX_reservation_fields_CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_reservation_fields_companies_CompanyId",
                table: "reservation_fields",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reservation_fields_companies_CompanyId",
                table: "reservation_fields");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "reservation_fields",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_reservation_fields_CompanyId",
                table: "reservation_fields",
                newName: "IX_reservation_fields_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_reservation_fields_users_UserId",
                table: "reservation_fields",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id");
        }
    }
}

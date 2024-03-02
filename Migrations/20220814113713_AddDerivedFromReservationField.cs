using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AddDerivedFromReservationField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DerivedFromFieldId",
                table: "reservation_fields",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservation_fields_DerivedFromFieldId",
                table: "reservation_fields",
                column: "DerivedFromFieldId");

            migrationBuilder.AddForeignKey(
                name: "FK_reservation_fields_reservation_fields_DerivedFromFieldId",
                table: "reservation_fields",
                column: "DerivedFromFieldId",
                principalTable: "reservation_fields",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reservation_fields_reservation_fields_DerivedFromFieldId",
                table: "reservation_fields");

            migrationBuilder.DropIndex(
                name: "IX_reservation_fields_DerivedFromFieldId",
                table: "reservation_fields");

            migrationBuilder.DropColumn(
                name: "DerivedFromFieldId",
                table: "reservation_fields");
        }
    }
}

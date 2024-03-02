using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AddCodeToReservation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "reservations",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "reservations");
        }
    }
}

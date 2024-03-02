using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AddAdditionalCOntactMailReservation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "additionalContactEmail",
                table: "reservations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "additionalContactEmail",
                table: "recurring_reservations",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "additionalContactEmail",
                table: "reservations");

            migrationBuilder.DropColumn(
                name: "additionalContactEmail",
                table: "recurring_reservations");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace Warehouses.Migrations
{
    public partial class AddPdfLinkToReservation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "pdfFileName",
                table: "reservations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pdfToken",
                table: "reservations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pdfFileName",
                table: "recurring_reservations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pdfToken",
                table: "recurring_reservations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "pdfFileName",
                table: "reservations");

            migrationBuilder.DropColumn(
                name: "pdfToken",
                table: "reservations");

            migrationBuilder.DropColumn(
                name: "pdfFileName",
                table: "recurring_reservations");

            migrationBuilder.DropColumn(
                name: "pdfToken",
                table: "recurring_reservations");
        }
    }
}

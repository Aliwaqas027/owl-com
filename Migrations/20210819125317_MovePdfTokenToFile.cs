using Microsoft.EntityFrameworkCore.Migrations;

namespace Warehouses.Migrations
{
    public partial class MovePdfTokenToFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "PdfToken",
                table: "files",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfToken",
                table: "files");

            migrationBuilder.AddColumn<string>(
                name: "pdfFileName",
                table: "reservations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pdfToken",
                table: "reservations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pdfFileName",
                table: "recurring_reservations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pdfToken",
                table: "recurring_reservations",
                type: "text",
                nullable: true);
        }
    }
}

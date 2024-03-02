using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AddMeaningToReservationField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HelpText",
                table: "reservation_fields",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpecialMeaning",
                table: "reservation_fields",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HelpText",
                table: "reservation_fields");

            migrationBuilder.DropColumn(
                name: "SpecialMeaning",
                table: "reservation_fields");
        }
    }
}

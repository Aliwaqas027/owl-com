using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class DateFormatForUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayDateFormat",
                table: "users",
                type: "text",
                defaultValue: "DD. MM. yyyy",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "DisplayTimeFormat",
                table: "users",
                type: "text",
                defaultValue: "HH:mm:ss",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayDateFormat",
                table: "users");

            migrationBuilder.DropColumn(
                name: "DisplayTimeFormat",
                table: "users");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace Warehouses.Migrations
{
    public partial class AddShowInEmailOption : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowInMail",
                table: "reservation_fields",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowInMail",
                table: "reservation_fields");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class CompanyAddProfileSetupNotice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowFirstTimeProfileSetupNotice",
                table: "companies",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowFirstTimeProfileSetupNotice",
                table: "companies");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace Warehouses.Migrations
{
    public partial class OptimapiPlanName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShareCode",
                table: "optimapi_plans");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "optimapi_plans",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "optimapi_plans");

            migrationBuilder.AddColumn<string>(
                name: "ShareCode",
                table: "optimapi_plans",
                type: "text",
                nullable: true);
        }
    }
}

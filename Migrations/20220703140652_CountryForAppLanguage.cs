using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class CountryForAppLanguage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "app_languages",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_app_languages_CountryId",
                table: "app_languages",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_app_languages_countries_CountryId",
                table: "app_languages",
                column: "CountryId",
                principalTable: "countries",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_app_languages_countries_CountryId",
                table: "app_languages");

            migrationBuilder.DropIndex(
                name: "IX_app_languages_CountryId",
                table: "app_languages");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "app_languages");
        }
    }
}

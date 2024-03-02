using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AddDefaultCompanyMailLanguage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultMailLanguageId",
                table: "companies",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_companies_DefaultMailLanguageId",
                table: "companies",
                column: "DefaultMailLanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_companies_app_languages_DefaultMailLanguageId",
                table: "companies",
                column: "DefaultMailLanguageId",
                principalTable: "app_languages",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_companies_app_languages_DefaultMailLanguageId",
                table: "companies");

            migrationBuilder.DropIndex(
                name: "IX_companies_DefaultMailLanguageId",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "DefaultMailLanguageId",
                table: "companies");
        }
    }
}

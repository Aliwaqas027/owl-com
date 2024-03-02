using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AddLanguageToFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LanguageId",
                table: "files",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_files_LanguageId",
                table: "files",
                column: "LanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_files_app_languages_LanguageId",
                table: "files",
                column: "LanguageId",
                principalTable: "app_languages",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_files_app_languages_LanguageId",
                table: "files");

            migrationBuilder.DropIndex(
                name: "IX_files_LanguageId",
                table: "files");

            migrationBuilder.DropColumn(
                name: "LanguageId",
                table: "files");
        }
    }
}

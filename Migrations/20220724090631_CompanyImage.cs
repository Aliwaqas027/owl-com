using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class CompanyImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "files",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_files_CompanyId",
                table: "files",
                column: "CompanyId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_files_companies_CompanyId",
                table: "files",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_files_companies_CompanyId",
                table: "files");

            migrationBuilder.DropIndex(
                name: "IX_files_CompanyId",
                table: "files");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "files");
        }
    }
}

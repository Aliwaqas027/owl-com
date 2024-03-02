using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Warehouses.Migrations
{
    public partial class SeparateSolutionFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RouteData",
                table: "optimapi_solutions");

            migrationBuilder.CreateTable(
                name: "optimapi_solution_files",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SolutionId = table.Column<int>(nullable: false),
                    Data = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_optimapi_solution_files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_optimapi_solution_files_optimapi_solutions_SolutionId",
                        column: x => x.SolutionId,
                        principalTable: "optimapi_solutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_optimapi_solution_files_SolutionId",
                table: "optimapi_solution_files",
                column: "SolutionId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "optimapi_solution_files");

            migrationBuilder.AddColumn<byte[]>(
                name: "RouteData",
                table: "optimapi_solutions",
                type: "bytea",
                nullable: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

namespace Warehouses.Migrations
{
    public partial class OptimapiPlans : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "optimapi_plans",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(nullable: false),
                    ShareCode = table.Column<string>(nullable: true),
                    Finished = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_optimapi_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_optimapi_plans_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "optimapi_solutions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanId = table.Column<int>(nullable: false),
                    Iteration = table.Column<int>(nullable: false),
                    Final = table.Column<bool>(nullable: false),
                    RouteData = table.Column<byte[]>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_optimapi_solutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_optimapi_solutions_optimapi_plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "optimapi_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_optimapi_plans_UserId",
                table: "optimapi_plans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_optimapi_solutions_PlanId",
                table: "optimapi_solutions",
                column: "PlanId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "optimapi_solutions");

            migrationBuilder.DropTable(
                name: "optimapi_plans");
        }
    }
}

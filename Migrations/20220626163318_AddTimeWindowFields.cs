using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AddTimeWindowFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookablePallets",
                table: "time_windows",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BookableSlots",
                table: "time_windows",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TimeWindowFieldsFilters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TimeWindowId = table.Column<int>(type: "integer", nullable: false),
                    ReservationFieldId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeWindowFieldsFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeWindowFieldsFilters_reservation_fields_ReservationField~",
                        column: x => x.ReservationFieldId,
                        principalTable: "reservation_fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimeWindowFieldsFilters_time_windows_TimeWindowId",
                        column: x => x.TimeWindowId,
                        principalTable: "time_windows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeWindowFieldsFilters_ReservationFieldId",
                table: "TimeWindowFieldsFilters",
                column: "ReservationFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeWindowFieldsFilters_TimeWindowId",
                table: "TimeWindowFieldsFilters",
                column: "TimeWindowId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeWindowFieldsFilters");

            migrationBuilder.DropColumn(
                name: "BookablePallets",
                table: "time_windows");

            migrationBuilder.DropColumn(
                name: "BookableSlots",
                table: "time_windows");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AddDoorFieldFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "door_fields_filter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoorId = table.Column<int>(type: "integer", nullable: false),
                    ReservationFieldId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_door_fields_filter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_door_fields_filter_doors_DoorId",
                        column: x => x.DoorId,
                        principalTable: "doors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_door_fields_filter_reservation_fields_ReservationFieldId",
                        column: x => x.ReservationFieldId,
                        principalTable: "reservation_fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_door_fields_filter_DoorId",
                table: "door_fields_filter",
                column: "DoorId");

            migrationBuilder.CreateIndex(
                name: "IX_door_fields_filter_ReservationFieldId",
                table: "door_fields_filter",
                column: "ReservationFieldId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "door_fields_filter");
        }
    }
}

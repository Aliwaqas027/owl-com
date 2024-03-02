using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Warehouses.Migrations
{
    public partial class AddReservationFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservationFields",
                table: "warehouses");

            migrationBuilder.CreateTable(
                name: "reservation_fields",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: true),
                    Required = table.Column<bool>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: true),
                    WarehouseId = table.Column<int>(nullable: true),
                    DoorId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservation_fields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reservation_fields_doors_DoorId",
                        column: x => x.DoorId,
                        principalTable: "doors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservation_fields_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservation_fields_warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reservation_fields_DoorId",
                table: "reservation_fields",
                column: "DoorId");

            migrationBuilder.CreateIndex(
                name: "IX_reservation_fields_UserId",
                table: "reservation_fields",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_reservation_fields_WarehouseId",
                table: "reservation_fields",
                column: "WarehouseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reservation_fields");

            migrationBuilder.AddColumn<object>(
                name: "ReservationFields",
                table: "warehouses",
                type: "jsonb",
                nullable: true);
        }
    }
}

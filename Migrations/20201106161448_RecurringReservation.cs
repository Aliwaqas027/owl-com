using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

namespace Warehouses.Migrations
{
    public partial class RecurringReservation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecurringReservationId",
                table: "files",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "recurring_reservations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CarrierId = table.Column<int>(nullable: false),
                    Start = table.Column<TimeSpan>(nullable: false),
                    End = table.Column<TimeSpan>(nullable: false),
                    FromDate = table.Column<DateTime>(nullable: true),
                    ToDate = table.Column<DateTime>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: true),
                    DoorId = table.Column<int>(nullable: true),
                    WarehouseId = table.Column<int>(nullable: true),
                    RecurrenceRule = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurring_reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recurring_reservations_users_CarrierId",
                        column: x => x.CarrierId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_recurring_reservations_doors_DoorId",
                        column: x => x.DoorId,
                        principalTable: "doors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recurring_reservations_warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_files_RecurringReservationId",
                table: "files",
                column: "RecurringReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_reservations_CarrierId",
                table: "recurring_reservations",
                column: "CarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_reservations_DoorId",
                table: "recurring_reservations",
                column: "DoorId");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_reservations_WarehouseId",
                table: "recurring_reservations",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_files_recurring_reservations_RecurringReservationId",
                table: "files",
                column: "RecurringReservationId",
                principalTable: "recurring_reservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_files_recurring_reservations_RecurringReservationId",
                table: "files");

            migrationBuilder.DropTable(
                name: "recurring_reservations");

            migrationBuilder.DropIndex(
                name: "IX_files_RecurringReservationId",
                table: "files");

            migrationBuilder.DropColumn(
                name: "RecurringReservationId",
                table: "files");
        }
    }
}

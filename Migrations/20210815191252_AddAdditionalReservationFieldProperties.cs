using Microsoft.EntityFrameworkCore.Migrations;
using System.Collections.Generic;

namespace Warehouses.Migrations
{
    public partial class AddAdditionalReservationFieldProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Default",
                table: "reservation_fields",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPalletsField",
                table: "reservation_fields",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Max",
                table: "reservation_fields",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Min",
                table: "reservation_fields",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "SelectValues",
                table: "reservation_fields",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SequenceNumber",
                table: "reservation_fields",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Default",
                table: "reservation_fields");

            migrationBuilder.DropColumn(
                name: "IsPalletsField",
                table: "reservation_fields");

            migrationBuilder.DropColumn(
                name: "Max",
                table: "reservation_fields");

            migrationBuilder.DropColumn(
                name: "Min",
                table: "reservation_fields");

            migrationBuilder.DropColumn(
                name: "SelectValues",
                table: "reservation_fields");

            migrationBuilder.DropColumn(
                name: "SequenceNumber",
                table: "reservation_fields");
        }
    }
}

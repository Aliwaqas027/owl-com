using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AddWarehouseProps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "warehouses",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "warehouses",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MaxArrivalInacurracy",
                table: "availabilities",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "WorkTimeFrom",
                table: "availabilities",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "WorkTimeTo",
                table: "availabilities",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "warehouses");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "warehouses");

            migrationBuilder.DropColumn(
                name: "MaxArrivalInacurracy",
                table: "availabilities");

            migrationBuilder.DropColumn(
                name: "WorkTimeFrom",
                table: "availabilities");

            migrationBuilder.DropColumn(
                name: "WorkTimeTo",
                table: "availabilities");
        }
    }
}

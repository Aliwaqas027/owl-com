using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AddReservationLanguage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LanguageId",
                table: "reservations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LanguageId",
                table: "recurring_reservations",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservations_LanguageId",
                table: "reservations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_reservations_LanguageId",
                table: "recurring_reservations",
                column: "LanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_recurring_reservations_app_languages_LanguageId",
                table: "recurring_reservations",
                column: "LanguageId",
                principalTable: "app_languages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_reservations_app_languages_LanguageId",
                table: "reservations",
                column: "LanguageId",
                principalTable: "app_languages",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_recurring_reservations_app_languages_LanguageId",
                table: "recurring_reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_reservations_app_languages_LanguageId",
                table: "reservations");

            migrationBuilder.DropIndex(
                name: "IX_reservations_LanguageId",
                table: "reservations");

            migrationBuilder.DropIndex(
                name: "IX_recurring_reservations_LanguageId",
                table: "recurring_reservations");

            migrationBuilder.DropColumn(
                name: "LanguageId",
                table: "reservations");

            migrationBuilder.DropColumn(
                name: "LanguageId",
                table: "recurring_reservations");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace Warehouses.Migrations
{
    public partial class UserConfirmedBy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConfirmedById",
                table: "users",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_ConfirmedById",
                table: "users",
                column: "ConfirmedById");

            migrationBuilder.AddForeignKey(
                name: "FK_users_users_ConfirmedById",
                table: "users",
                column: "ConfirmedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_users_ConfirmedById",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_ConfirmedById",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ConfirmedById",
                table: "users");
        }
    }
}

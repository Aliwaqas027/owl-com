using Microsoft.EntityFrameworkCore.Migrations;

namespace Warehouses.Migrations
{
    public partial class AddUserMailSendingData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MailSendingData",
                table: "users",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MailSendingData",
                table: "users");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class RemoveMailTexts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MailSendingTexts",
                table: "users");

            migrationBuilder.DropColumn(
                name: "MailSubjectOrder",
                table: "users");

            migrationBuilder.DropColumn(
                name: "MailSendingTexts",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "MailSubjectOrder",
                table: "companies");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MailSendingTexts",
                table: "users",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailSubjectOrder",
                table: "users",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailSendingTexts",
                table: "companies",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailSubjectOrder",
                table: "companies",
                type: "jsonb",
                nullable: true);
        }
    }
}

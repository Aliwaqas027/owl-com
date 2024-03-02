using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AddSendContractInMailCompany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SendContractInMail",
                table: "companies",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SendContractInMail",
                table: "companies");
        }
    }
}

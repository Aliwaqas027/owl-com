using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class ModifyCompanies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_contact_mails_users_UserId",
                table: "contact_mails");

            migrationBuilder.DropForeignKey(
                name: "FK_warehouses_users_OwnerId",
                table: "warehouses");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "warehouses",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_warehouses_OwnerId",
                table: "warehouses",
                newName: "IX_warehouses_CompanyId");

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "warehouses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "files",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "contact_mails",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "contact_mails",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailSendingData",
                table: "companies",
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

            migrationBuilder.CreateIndex(
                name: "IX_warehouses_CreatedById",
                table: "warehouses",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_files_WarehouseId",
                table: "files",
                column: "WarehouseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contact_mails_CompanyId",
                table: "contact_mails",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_contact_mails_companies_CompanyId",
                table: "contact_mails",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_contact_mails_users_UserId",
                table: "contact_mails",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_files_warehouses_WarehouseId",
                table: "files",
                column: "WarehouseId",
                principalTable: "warehouses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_warehouses_companies_CompanyId",
                table: "warehouses",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_warehouses_users_CreatedById",
                table: "warehouses",
                column: "CreatedById",
                principalTable: "users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_contact_mails_companies_CompanyId",
                table: "contact_mails");

            migrationBuilder.DropForeignKey(
                name: "FK_contact_mails_users_UserId",
                table: "contact_mails");

            migrationBuilder.DropForeignKey(
                name: "FK_files_warehouses_WarehouseId",
                table: "files");

            migrationBuilder.DropForeignKey(
                name: "FK_warehouses_companies_CompanyId",
                table: "warehouses");

            migrationBuilder.DropForeignKey(
                name: "FK_warehouses_users_CreatedById",
                table: "warehouses");

            migrationBuilder.DropIndex(
                name: "IX_warehouses_CreatedById",
                table: "warehouses");

            migrationBuilder.DropIndex(
                name: "IX_files_WarehouseId",
                table: "files");

            migrationBuilder.DropIndex(
                name: "IX_contact_mails_CompanyId",
                table: "contact_mails");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "warehouses");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "files");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "contact_mails");

            migrationBuilder.DropColumn(
                name: "MailSendingData",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "MailSendingTexts",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "MailSubjectOrder",
                table: "companies");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "warehouses",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_warehouses_CompanyId",
                table: "warehouses",
                newName: "IX_warehouses_OwnerId");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "contact_mails",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_contact_mails_users_UserId",
                table: "contact_mails",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_warehouses_users_OwnerId",
                table: "warehouses",
                column: "OwnerId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Warehouses.Migrations
{
    public partial class AddEmailTemplates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyAttachmentId",
                table: "files",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DoorAttachmentId",
                table: "files",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseAttachmentId",
                table: "files",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "email_templates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    SubjectTemplate = table.Column<string>(type: "text", nullable: true),
                    ContentTemplate = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_email_templates_app_languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "app_languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_email_templates_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_files_CompanyAttachmentId",
                table: "files",
                column: "CompanyAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_files_DoorAttachmentId",
                table: "files",
                column: "DoorAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_files_WarehouseAttachmentId",
                table: "files",
                column: "WarehouseAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_email_templates_CompanyId",
                table: "email_templates",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_email_templates_LanguageId",
                table: "email_templates",
                column: "LanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_files_companies_CompanyAttachmentId",
                table: "files",
                column: "CompanyAttachmentId",
                principalTable: "companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_files_doors_DoorAttachmentId",
                table: "files",
                column: "DoorAttachmentId",
                principalTable: "doors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_files_warehouses_WarehouseAttachmentId",
                table: "files",
                column: "WarehouseAttachmentId",
                principalTable: "warehouses",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_files_companies_CompanyAttachmentId",
                table: "files");

            migrationBuilder.DropForeignKey(
                name: "FK_files_doors_DoorAttachmentId",
                table: "files");

            migrationBuilder.DropForeignKey(
                name: "FK_files_warehouses_WarehouseAttachmentId",
                table: "files");

            migrationBuilder.DropTable(
                name: "email_templates");

            migrationBuilder.DropIndex(
                name: "IX_files_CompanyAttachmentId",
                table: "files");

            migrationBuilder.DropIndex(
                name: "IX_files_DoorAttachmentId",
                table: "files");

            migrationBuilder.DropIndex(
                name: "IX_files_WarehouseAttachmentId",
                table: "files");

            migrationBuilder.DropColumn(
                name: "CompanyAttachmentId",
                table: "files");

            migrationBuilder.DropColumn(
                name: "DoorAttachmentId",
                table: "files");

            migrationBuilder.DropColumn(
                name: "WarehouseAttachmentId",
                table: "files");
        }
    }
}

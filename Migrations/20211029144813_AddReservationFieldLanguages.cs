using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Warehouses.Migrations
{
    public partial class AddReservationFieldLanguages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "reservation_fields");

            migrationBuilder.CreateTable(
                name: "app_languages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(nullable: true),
                    subdomain = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reservation_field_names",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(nullable: true),
                    fieldId = table.Column<int>(nullable: true),
                    languageId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservation_field_names", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reservation_field_names_reservation_fields_fieldId",
                        column: x => x.fieldId,
                        principalTable: "reservation_fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservation_field_names_app_languages_languageId",
                        column: x => x.languageId,
                        principalTable: "app_languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reservation_field_names_fieldId",
                table: "reservation_field_names",
                column: "fieldId");

            migrationBuilder.CreateIndex(
                name: "IX_reservation_field_names_languageId",
                table: "reservation_field_names",
                column: "languageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reservation_field_names");

            migrationBuilder.DropTable(
                name: "app_languages");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "reservation_fields",
                type: "text",
                nullable: true);
        }
    }
}

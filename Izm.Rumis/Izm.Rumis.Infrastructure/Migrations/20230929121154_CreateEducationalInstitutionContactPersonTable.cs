using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    public partial class CreateEducationalInstitutionContactPersonTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EducationalInstitutionContactPersons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    JobPosition = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EducationalInstitutionId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ModifiedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationalInstitutionContactPersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EducationalInstitutionContactPersons_EducationalInstitutions~",
                        column: x => x.EducationalInstitutionId,
                        principalTable: "EducationalInstitutions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EducationalInstitutionContactPersons_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EducationalInstitutionContactPersons_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EducationalInstitutionContactPersons_CreatedById",
                table: "EducationalInstitutionContactPersons",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_EducationalInstitutionContactPersons_EducationalInstitutionId",
                table: "EducationalInstitutionContactPersons",
                column: "EducationalInstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationalInstitutionContactPersons_ModifiedById",
                table: "EducationalInstitutionContactPersons",
                column: "ModifiedById");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EducationalInstitutionContactPersons");
        }
    }
}

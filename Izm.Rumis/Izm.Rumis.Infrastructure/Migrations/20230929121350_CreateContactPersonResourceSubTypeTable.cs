using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    public partial class CreateContactPersonResourceSubTypeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactPersonResourceSubTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EducationalInstitutionContactPersonId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ResourceSubTypeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ModifiedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactPersonResourceSubTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactPersonResourceSubTypes_Classifiers_ResourceSubTypeId",
                        column: x => x.ResourceSubTypeId,
                        principalTable: "Classifiers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContactPersonResourceSubTypes_EducationalInstitutionContactP~",
                        column: x => x.EducationalInstitutionContactPersonId,
                        principalTable: "EducationalInstitutionContactPersons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContactPersonResourceSubTypes_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContactPersonResourceSubTypes_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPersonResourceSubTypes_CreatedById",
                table: "ContactPersonResourceSubTypes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPersonResourceSubTypes_EducationalInstitutionContactP~",
                table: "ContactPersonResourceSubTypes",
                column: "EducationalInstitutionContactPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPersonResourceSubTypes_ModifiedById",
                table: "ContactPersonResourceSubTypes",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPersonResourceSubTypes_ResourceSubTypeId",
                table: "ContactPersonResourceSubTypes",
                column: "ResourceSubTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactPersonResourceSubTypes");
        }
    }
}

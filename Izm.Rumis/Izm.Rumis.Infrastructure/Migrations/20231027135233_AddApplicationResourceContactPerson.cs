using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationResourceContactPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationResourceContactPersons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ApplicationResourceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EducationalInstitutionContactPersonId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ModifiedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationResourceContactPersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationResourceContactPersons_ApplicationResources_Appli~",
                        column: x => x.ApplicationResourceId,
                        principalTable: "ApplicationResources",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationResourceContactPersons_EducationalInstitutionCont~",
                        column: x => x.EducationalInstitutionContactPersonId,
                        principalTable: "EducationalInstitutionContactPersons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationResourceContactPersons_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationResourceContactPersons_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResourceContactPersons_ApplicationResourceId",
                table: "ApplicationResourceContactPersons",
                column: "ApplicationResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResourceContactPersons_CreatedById",
                table: "ApplicationResourceContactPersons",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResourceContactPersons_EducationalInstitutionCont~",
                table: "ApplicationResourceContactPersons",
                column: "EducationalInstitutionContactPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResourceContactPersons_ModifiedById",
                table: "ApplicationResourceContactPersons",
                column: "ModifiedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationResourceContactPersons");
        }
    }
}

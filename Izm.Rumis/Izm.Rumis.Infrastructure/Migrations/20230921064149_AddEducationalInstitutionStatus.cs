using System;
using Izm.Rumis.Infrastructure.Common;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    public partial class AddEducationalInstitutionStatus : Migration
    {
        private Guid ClassifierId = Guid.Parse("977b3aa8-de36-46a7-8fee-3db45123c967");

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var dateTimeValue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            migrationBuilder.Sql($"INSERT IGNORE INTO Classifiers VALUES ('{ClassifierId}', 'educational_institution_status', 'disabled', 'Neaktīvs', NULL, NULL, NULL, NULL, 0, 0, '{dateTimeValue}', '{dateTimeValue}', '{UserIds.Application}', '{UserIds.Application}', NULL, 'Country', NULL);");

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                table: "EducationalInstitutions",
                type: "char(36)",
                nullable: false,
                defaultValue: ClassifierId,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_EducationalInstitutions_StatusId",
                table: "EducationalInstitutions",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalInstitutions_Classifiers_StatusId",
                table: "EducationalInstitutions",
                column: "StatusId",
                principalTable: "Classifiers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropForeignKey(
                name: "FK_EducationalInstitutions_Classifiers_StatusId",
                table: "EducationalInstitutions");

            migrationBuilder.DropIndex(
                name: "IX_EducationalInstitutions_StatusId",
                table: "EducationalInstitutions");

            migrationBuilder.Sql($"DELETE FROM Classifiers WHERE Id = '{ClassifierId}';");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "EducationalInstitutions");
        }
    }
}

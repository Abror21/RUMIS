using Izm.Rumis.Infrastructure.Common;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFieldsToEducInstAndEducInstContPers : Migration
    {
        private Guid ClassifierId = Guid.Parse("c92506f8-036d-4d9d-9efe-7b607f6fac5b");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var dateTimeValue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            migrationBuilder.Sql($"INSERT IGNORE INTO Classifiers VALUES ('{ClassifierId}', 'educational_institution_job_position', 'ict_contact', 'IKT kontaktpersona', NULL, NULL, NULL, NULL, 0, 0, '{dateTimeValue}', '{dateTimeValue}', '{UserIds.Application}', '{UserIds.Application}', NULL, 'Country', NULL);");

            migrationBuilder.DropColumn(
                name: "JobPosition",
                table: "EducationalInstitutionContactPersons");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "EducationalInstitutions",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "EducationalInstitutions",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "EducationalInstitutions",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "EducationalInstitutions",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "EducationalInstitutions",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "EducationalInstitutionContactPersons",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "JobPositionId",
                table: "EducationalInstitutionContactPersons",
                type: "char(36)",
                nullable: false,
                defaultValue: ClassifierId,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_EducationalInstitutionContactPersons_JobPositionId",
                table: "EducationalInstitutionContactPersons",
                column: "JobPositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalInstitutionContactPersons_Classifiers_JobPosition~",
                table: "EducationalInstitutionContactPersons",
                column: "JobPositionId",
                principalTable: "Classifiers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationalInstitutionContactPersons_Classifiers_JobPosition~",
                table: "EducationalInstitutionContactPersons");

            migrationBuilder.DropIndex(
                name: "IX_EducationalInstitutionContactPersons_JobPositionId",
                table: "EducationalInstitutionContactPersons");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "City",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "District",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "EducationalInstitutionContactPersons");

            migrationBuilder.DropColumn(
                name: "JobPositionId",
                table: "EducationalInstitutionContactPersons");

            migrationBuilder.AddColumn<string>(
                name: "JobPosition",
                table: "EducationalInstitutionContactPersons",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}

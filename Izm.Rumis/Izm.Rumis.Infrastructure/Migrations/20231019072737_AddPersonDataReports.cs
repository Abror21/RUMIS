using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonDataReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EducationalInstitutionId",
                table: "GdprAudits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "GdprAudits",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PersonDataReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PrivatePersonlIdentifier = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReasonId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ModifiedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonDataReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonDataReports_Classifiers_ReasonId",
                        column: x => x.ReasonId,
                        principalTable: "Classifiers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonDataReports_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonDataReports_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonDataReports_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GdprAudits_EducationalInstitutionId",
                table: "GdprAudits",
                column: "EducationalInstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_GdprAudits_SupervisorId",
                table: "GdprAudits",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonDataReports_CreatedById",
                table: "PersonDataReports",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PersonDataReports_ModifiedById",
                table: "PersonDataReports",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_PersonDataReports_ReasonId",
                table: "PersonDataReports",
                column: "ReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonDataReports_UserProfileId",
                table: "PersonDataReports",
                column: "UserProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_GdprAudits_EducationalInstitutions_EducationalInstitutionId",
                table: "GdprAudits",
                column: "EducationalInstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GdprAudits_Supervisors_SupervisorId",
                table: "GdprAudits",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GdprAudits_EducationalInstitutions_EducationalInstitutionId",
                table: "GdprAudits");

            migrationBuilder.DropForeignKey(
                name: "FK_GdprAudits_Supervisors_SupervisorId",
                table: "GdprAudits");

            migrationBuilder.DropTable(
                name: "PersonDataReports");

            migrationBuilder.DropIndex(
                name: "IX_GdprAudits_EducationalInstitutionId",
                table: "GdprAudits");

            migrationBuilder.DropIndex(
                name: "IX_GdprAudits_SupervisorId",
                table: "GdprAudits");

            migrationBuilder.DropColumn(
                name: "EducationalInstitutionId",
                table: "GdprAudits");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "GdprAudits");
        }
    }
}

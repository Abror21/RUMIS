using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddForeginKeysToLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // UserId field at this point might be populated with
            // faulty data such as '', 'NULL', 'PK:XXXXXXXXXXX'...
            //
            // Threrefore this SQL is necessary for the foreign key to be created successfully!
            migrationBuilder.Sql("UPDATE Log SET UserId = NULL WHERE UserId IN ('', 'NULL') OR UserId LIKE 'PK:%'");

            migrationBuilder.CreateIndex(
                name: "IX_Log_EducationalInstitutionId",
                table: "Log",
                column: "EducationalInstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_Log_PersonId",
                table: "Log",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Log_SupervisorId",
                table: "Log",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_Log_UserId",
                table: "Log",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Log_UserProfileId",
                table: "Log",
                column: "UserProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Log_EducationalInstitutions_EducationalInstitutionId",
                table: "Log",
                column: "EducationalInstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Log_PersonTechnicals_PersonId",
                table: "Log",
                column: "PersonId",
                principalTable: "PersonTechnicals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Log_Supervisors_SupervisorId",
                table: "Log",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Log_UserProfiles_UserProfileId",
                table: "Log",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Log_Users_UserId",
                table: "Log",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Log_EducationalInstitutions_EducationalInstitutionId",
                table: "Log");

            migrationBuilder.DropForeignKey(
                name: "FK_Log_PersonTechnicals_PersonId",
                table: "Log");

            migrationBuilder.DropForeignKey(
                name: "FK_Log_Supervisors_SupervisorId",
                table: "Log");

            migrationBuilder.DropForeignKey(
                name: "FK_Log_UserProfiles_UserProfileId",
                table: "Log");

            migrationBuilder.DropForeignKey(
                name: "FK_Log_Users_UserId",
                table: "Log");

            migrationBuilder.DropIndex(
                name: "IX_Log_EducationalInstitutionId",
                table: "Log");

            migrationBuilder.DropIndex(
                name: "IX_Log_PersonId",
                table: "Log");

            migrationBuilder.DropIndex(
                name: "IX_Log_SupervisorId",
                table: "Log");

            migrationBuilder.DropIndex(
                name: "IX_Log_UserId",
                table: "Log");

            migrationBuilder.DropIndex(
                name: "IX_Log_UserProfileId",
                table: "Log");
        }
    }
}

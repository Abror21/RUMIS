using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    public partial class AddClassifierPermissionType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EducationalInstitutionId",
                table: "Classifiers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermissionType",
                table: "Classifiers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "Classifiers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classifiers_EducationalInstitutionId",
                table: "Classifiers",
                column: "EducationalInstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_Classifiers_SupervisorId",
                table: "Classifiers",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classifiers_EducationalInstitutions_EducationalInstitutionId",
                table: "Classifiers",
                column: "EducationalInstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Classifiers_Supervisors_SupervisorId",
                table: "Classifiers",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classifiers_EducationalInstitutions_EducationalInstitutionId",
                table: "Classifiers");

            migrationBuilder.DropForeignKey(
                name: "FK_Classifiers_Supervisors_SupervisorId",
                table: "Classifiers");

            migrationBuilder.DropIndex(
                name: "IX_Classifiers_EducationalInstitutionId",
                table: "Classifiers");

            migrationBuilder.DropIndex(
                name: "IX_Classifiers_SupervisorId",
                table: "Classifiers");

            migrationBuilder.DropColumn(
                name: "EducationalInstitutionId",
                table: "Classifiers");

            migrationBuilder.DropColumn(
                name: "PermissionType",
                table: "Classifiers");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "Classifiers");
        }
    }
}

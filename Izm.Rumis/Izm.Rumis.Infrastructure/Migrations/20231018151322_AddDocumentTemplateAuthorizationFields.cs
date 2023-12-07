using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentTemplateAuthorizationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EducationalInstitutionId",
                table: "DocumentTemplates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermissionType",
                table: "DocumentTemplates",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "DocumentTemplates",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTemplates_EducationalInstitutionId",
                table: "DocumentTemplates",
                column: "EducationalInstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTemplates_SupervisorId",
                table: "DocumentTemplates",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTemplates_EducationalInstitutions_EducationalInstitu~",
                table: "DocumentTemplates",
                column: "EducationalInstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTemplates_Supervisors_SupervisorId",
                table: "DocumentTemplates",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTemplates_EducationalInstitutions_EducationalInstitu~",
                table: "DocumentTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTemplates_Supervisors_SupervisorId",
                table: "DocumentTemplates");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTemplates_EducationalInstitutionId",
                table: "DocumentTemplates");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTemplates_SupervisorId",
                table: "DocumentTemplates");

            migrationBuilder.DropColumn(
                name: "EducationalInstitutionId",
                table: "DocumentTemplates");

            migrationBuilder.DropColumn(
                name: "PermissionType",
                table: "DocumentTemplates");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "DocumentTemplates");
        }
    }
}

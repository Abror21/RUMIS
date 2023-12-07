using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    public partial class RefactoringMunicipalityToFounder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationalInstitutions_Municipalities_MunicipalityId",
                table: "EducationalInstitutions");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonContacts_Classifiers_ContactTypeId",
                table: "PersonContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonContacts_PersonTechnicals_PersonTechnicalId",
                table: "PersonContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonContacts_Users_CreatedById",
                table: "PersonContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonContacts_Users_ModifiedById",
                table: "PersonContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_Municipalities_MunicipalityId",
                table: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Municipalities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonContacts",
                table: "PersonContacts");

            migrationBuilder.RenameTable(
                name: "PersonContacts",
                newName: "PersonContact");

            migrationBuilder.RenameColumn(
                name: "MunicipalityId",
                table: "UserProfiles",
                newName: "FounderId");

            migrationBuilder.RenameIndex(
                name: "IX_UserProfiles_MunicipalityId",
                table: "UserProfiles",
                newName: "IX_UserProfiles_FounderId");

            migrationBuilder.RenameColumn(
                name: "MunicipalityId",
                table: "EducationalInstitutions",
                newName: "FounderId");

            migrationBuilder.RenameIndex(
                name: "IX_EducationalInstitutions_MunicipalityId",
                table: "EducationalInstitutions",
                newName: "IX_EducationalInstitutions_FounderId");

            migrationBuilder.RenameIndex(
                name: "IX_PersonContacts_PersonTechnicalId",
                table: "PersonContact",
                newName: "IX_PersonContact_PersonTechnicalId");

            migrationBuilder.RenameIndex(
                name: "IX_PersonContacts_ModifiedById",
                table: "PersonContact",
                newName: "IX_PersonContact_ModifiedById");

            migrationBuilder.RenameIndex(
                name: "IX_PersonContacts_CreatedById",
                table: "PersonContact",
                newName: "IX_PersonContact_CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_PersonContacts_ContactTypeId",
                table: "PersonContact",
                newName: "IX_PersonContact_ContactTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonContact",
                table: "PersonContact",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Founders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ModifiedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Founders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Founders_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Founders_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Founders_CreatedById",
                table: "Founders",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Founders_ModifiedById",
                table: "Founders",
                column: "ModifiedById");

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalInstitutions_Founders_FounderId",
                table: "EducationalInstitutions",
                column: "FounderId",
                principalTable: "Founders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContact_Classifiers_ContactTypeId",
                table: "PersonContact",
                column: "ContactTypeId",
                principalTable: "Classifiers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContact_PersonTechnicals_PersonTechnicalId",
                table: "PersonContact",
                column: "PersonTechnicalId",
                principalTable: "PersonTechnicals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContact_Users_CreatedById",
                table: "PersonContact",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContact_Users_ModifiedById",
                table: "PersonContact",
                column: "ModifiedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_Founders_FounderId",
                table: "UserProfiles",
                column: "FounderId",
                principalTable: "Founders",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationalInstitutions_Founders_FounderId",
                table: "EducationalInstitutions");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonContact_Classifiers_ContactTypeId",
                table: "PersonContact");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonContact_PersonTechnicals_PersonTechnicalId",
                table: "PersonContact");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonContact_Users_CreatedById",
                table: "PersonContact");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonContact_Users_ModifiedById",
                table: "PersonContact");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_Founders_FounderId",
                table: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Founders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonContact",
                table: "PersonContact");

            migrationBuilder.RenameTable(
                name: "PersonContact",
                newName: "PersonContacts");

            migrationBuilder.RenameColumn(
                name: "FounderId",
                table: "UserProfiles",
                newName: "MunicipalityId");

            migrationBuilder.RenameIndex(
                name: "IX_UserProfiles_FounderId",
                table: "UserProfiles",
                newName: "IX_UserProfiles_MunicipalityId");

            migrationBuilder.RenameColumn(
                name: "FounderId",
                table: "EducationalInstitutions",
                newName: "MunicipalityId");

            migrationBuilder.RenameIndex(
                name: "IX_EducationalInstitutions_FounderId",
                table: "EducationalInstitutions",
                newName: "IX_EducationalInstitutions_MunicipalityId");

            migrationBuilder.RenameIndex(
                name: "IX_PersonContact_PersonTechnicalId",
                table: "PersonContacts",
                newName: "IX_PersonContacts_PersonTechnicalId");

            migrationBuilder.RenameIndex(
                name: "IX_PersonContact_ModifiedById",
                table: "PersonContacts",
                newName: "IX_PersonContacts_ModifiedById");

            migrationBuilder.RenameIndex(
                name: "IX_PersonContact_CreatedById",
                table: "PersonContacts",
                newName: "IX_PersonContacts_CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_PersonContact_ContactTypeId",
                table: "PersonContacts",
                newName: "IX_PersonContacts_ContactTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonContacts",
                table: "PersonContacts",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Municipalities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ModifiedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipalities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Municipalities_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Municipalities_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_CreatedById",
                table: "Municipalities",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_ModifiedById",
                table: "Municipalities",
                column: "ModifiedById");

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalInstitutions_Municipalities_MunicipalityId",
                table: "EducationalInstitutions",
                column: "MunicipalityId",
                principalTable: "Municipalities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContacts_Classifiers_ContactTypeId",
                table: "PersonContacts",
                column: "ContactTypeId",
                principalTable: "Classifiers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContacts_PersonTechnicals_PersonTechnicalId",
                table: "PersonContacts",
                column: "PersonTechnicalId",
                principalTable: "PersonTechnicals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContacts_Users_CreatedById",
                table: "PersonContacts",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContacts_Users_ModifiedById",
                table: "PersonContacts",
                column: "ModifiedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_Municipalities_MunicipalityId",
                table: "UserProfiles",
                column: "MunicipalityId",
                principalTable: "Municipalities",
                principalColumn: "Id");
        }
    }
}

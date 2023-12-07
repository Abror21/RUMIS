using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    public partial class RefactoringFounderToSupervisor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationalInstitutions_Founders_FounderId",
                table: "EducationalInstitutions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_Founders_FounderId",
                table: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Founders");

            migrationBuilder.RenameColumn(
                name: "FounderId",
                table: "UserProfiles",
                newName: "SupervisorId");

            migrationBuilder.RenameIndex(
                name: "IX_UserProfiles_FounderId",
                table: "UserProfiles",
                newName: "IX_UserProfiles_SupervisorId");

            migrationBuilder.RenameColumn(
                name: "FounderId",
                table: "EducationalInstitutions",
                newName: "SupervisorId");

            migrationBuilder.RenameIndex(
                name: "IX_EducationalInstitutions_FounderId",
                table: "EducationalInstitutions",
                newName: "IX_EducationalInstitutions_SupervisorId");

            migrationBuilder.CreateTable(
                name: "Supervisors",
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
                    table.PrimaryKey("PK_Supervisors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Supervisors_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Supervisors_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Supervisors_CreatedById",
                table: "Supervisors",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Supervisors_ModifiedById",
                table: "Supervisors",
                column: "ModifiedById");

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalInstitutions_Supervisors_SupervisorId",
                table: "EducationalInstitutions",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_Supervisors_SupervisorId",
                table: "UserProfiles",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationalInstitutions_Supervisors_SupervisorId",
                table: "EducationalInstitutions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_Supervisors_SupervisorId",
                table: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Supervisors");

            migrationBuilder.RenameColumn(
                name: "SupervisorId",
                table: "UserProfiles",
                newName: "FounderId");

            migrationBuilder.RenameIndex(
                name: "IX_UserProfiles_SupervisorId",
                table: "UserProfiles",
                newName: "IX_UserProfiles_FounderId");

            migrationBuilder.RenameColumn(
                name: "SupervisorId",
                table: "EducationalInstitutions",
                newName: "FounderId");

            migrationBuilder.RenameIndex(
                name: "IX_EducationalInstitutions_SupervisorId",
                table: "EducationalInstitutions",
                newName: "IX_EducationalInstitutions_FounderId");

            migrationBuilder.CreateTable(
                name: "Founders",
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
                name: "FK_UserProfiles_Founders_FounderId",
                table: "UserProfiles",
                column: "FounderId",
                principalTable: "Founders",
                principalColumn: "Id");
        }
    }
}

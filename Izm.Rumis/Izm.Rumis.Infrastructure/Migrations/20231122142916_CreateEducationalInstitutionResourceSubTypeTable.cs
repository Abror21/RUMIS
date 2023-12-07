using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateEducationalInstitutionResourceSubTypeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EducationalInstitutionResourceSubTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EducationalInstitutionId = table.Column<int>(type: "int", nullable: false),
                    ResourceSubTypeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TargetPersonGroupTypeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ModifiedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationalInstitutionResourceSubTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EducationalInstitutionResourceSubTypes_Classifiers_ResourceS~",
                        column: x => x.ResourceSubTypeId,
                        principalTable: "Classifiers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EducationalInstitutionResourceSubTypes_Classifiers_TargetPer~",
                        column: x => x.TargetPersonGroupTypeId,
                        principalTable: "Classifiers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EducationalInstitutionResourceSubTypes_EducationalInstitutio~",
                        column: x => x.EducationalInstitutionId,
                        principalTable: "EducationalInstitutions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EducationalInstitutionResourceSubTypes_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EducationalInstitutionResourceSubTypes_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EducationalInstitutionResourceSubTypes_CreatedById",
                table: "EducationalInstitutionResourceSubTypes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_EducationalInstitutionResourceSubTypes_EducationalInstitutio~",
                table: "EducationalInstitutionResourceSubTypes",
                column: "EducationalInstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationalInstitutionResourceSubTypes_ModifiedById",
                table: "EducationalInstitutionResourceSubTypes",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_EducationalInstitutionResourceSubTypes_ResourceSubTypeId",
                table: "EducationalInstitutionResourceSubTypes",
                column: "ResourceSubTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationalInstitutionResourceSubTypes_TargetPersonGroupType~",
                table: "EducationalInstitutionResourceSubTypes",
                column: "TargetPersonGroupTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EducationalInstitutionResourceSubTypes");
        }
    }
}

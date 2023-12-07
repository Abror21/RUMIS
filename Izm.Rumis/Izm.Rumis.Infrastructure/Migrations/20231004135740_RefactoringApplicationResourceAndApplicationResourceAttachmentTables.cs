using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    public partial class RefactoringApplicationResourceAndApplicationResourceAttachmentTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationResourcePna");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ApplicationResources",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PNANumber",
                table: "ApplicationResources",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "PNAStatusId",
                table: "ApplicationResources",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "ApplicationResourceAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DocumentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ApplicationResourceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DocumentTypeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DocumentTemplateId = table.Column<int>(type: "int", nullable: false),
                    FileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ModifiedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationResourceAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationResourceAttachments_ApplicationResources_Applicat~",
                        column: x => x.ApplicationResourceId,
                        principalTable: "ApplicationResources",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationResourceAttachments_Classifiers_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "Classifiers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationResourceAttachments_DocumentTemplates_DocumentTem~",
                        column: x => x.DocumentTemplateId,
                        principalTable: "DocumentTemplates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationResourceAttachments_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationResourceAttachments_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationResourceAttachments_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResources_PNAStatusId",
                table: "ApplicationResources",
                column: "PNAStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResourceAttachments_ApplicationResourceId",
                table: "ApplicationResourceAttachments",
                column: "ApplicationResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResourceAttachments_CreatedById",
                table: "ApplicationResourceAttachments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResourceAttachments_DocumentTemplateId",
                table: "ApplicationResourceAttachments",
                column: "DocumentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResourceAttachments_DocumentTypeId",
                table: "ApplicationResourceAttachments",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResourceAttachments_FileId",
                table: "ApplicationResourceAttachments",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResourceAttachments_ModifiedById",
                table: "ApplicationResourceAttachments",
                column: "ModifiedById");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationResources_Classifiers_PNAStatusId",
                table: "ApplicationResources",
                column: "PNAStatusId",
                principalTable: "Classifiers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationResources_Classifiers_PNAStatusId",
                table: "ApplicationResources");

            migrationBuilder.DropTable(
                name: "ApplicationResourceAttachments");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationResources_PNAStatusId",
                table: "ApplicationResources");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ApplicationResources");

            migrationBuilder.DropColumn(
                name: "PNANumber",
                table: "ApplicationResources");

            migrationBuilder.DropColumn(
                name: "PNAStatusId",
                table: "ApplicationResources");

            migrationBuilder.CreateTable(
                name: "ApplicationResourcePna",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ApplicationResourceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DocumentTemplateId = table.Column<int>(type: "int", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ModifiedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PNAStatusId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime", nullable: false),
                    PNADate = table.Column<DateOnly>(type: "date", nullable: false),
                    PNANumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationResourcePna", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationResourcePna_ApplicationResources_ApplicationResou~",
                        column: x => x.ApplicationResourceId,
                        principalTable: "ApplicationResources",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationResourcePna_Classifiers_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "Classifiers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationResourcePna_Classifiers_PNAStatusId",
                        column: x => x.PNAStatusId,
                        principalTable: "Classifiers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationResourcePna_DocumentTemplates_DocumentTemplateId",
                        column: x => x.DocumentTemplateId,
                        principalTable: "DocumentTemplates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationResourcePna_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationResourcePna_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationResourcePna_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResourcePna_ApplicationResourceId",
                table: "ApplicationResourcePna",
                column: "ApplicationResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResourcePna_CreatedById",
                table: "ApplicationResourcePna",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResourcePna_DocumentTemplateId",
                table: "ApplicationResourcePna",
                column: "DocumentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResourcePna_DocumentTypeId",
                table: "ApplicationResourcePna",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResourcePna_FileId",
                table: "ApplicationResourcePna",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResourcePna_ModifiedById",
                table: "ApplicationResourcePna",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResourcePna_PNAStatusId",
                table: "ApplicationResourcePna",
                column: "PNAStatusId");
        }
    }
}

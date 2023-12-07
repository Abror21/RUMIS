using System;
using Izm.Rumis.Infrastructure.Common;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DocumentTemplateUpdate : Migration
    {
        private Guid ClassifierId = Guid.Parse("a0c428fe-4bc1-4c2b-8b97-9231ea49891b");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var dateTimeValue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            migrationBuilder.Sql($"INSERT IGNORE INTO Classifiers VALUES ('{ClassifierId}', 'resource_type', 'computer', 'Dators', NULL, NULL, NULL, NULL, 0, 0, '{dateTimeValue}', '{dateTimeValue}', '{UserIds.Application}', '{UserIds.Application}', NULL, 'Country', NULL);");

            migrationBuilder.AddColumn<string>(
                name: "Hyperlink",
                table: "DocumentTemplates",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "ResourceTypeId",
                table: "DocumentTemplates",
                type: "char(36)",
                nullable: false,
                defaultValue: ClassifierId,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTemplates_ResourceTypeId",
                table: "DocumentTemplates",
                column: "ResourceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTemplates_Classifiers_ResourceTypeId",
                table: "DocumentTemplates",
                column: "ResourceTypeId",
                principalTable: "Classifiers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTemplates_Classifiers_ResourceTypeId",
                table: "DocumentTemplates");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTemplates_ResourceTypeId",
                table: "DocumentTemplates");

            migrationBuilder.DropColumn(
                name: "Hyperlink",
                table: "DocumentTemplates");

            migrationBuilder.DropColumn(
                name: "ResourceTypeId",
                table: "DocumentTemplates");
        }
    }
}

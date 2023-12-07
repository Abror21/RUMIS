using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    public partial class AddColumnsToUserProfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfigurationInfo",
                table: "UserProfiles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "InstitutionId",
                table: "UserProfiles",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "UserProfiles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProfileCreationDocumentDate",
                table: "UserProfiles",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileCreationDocumentNumber",
                table: "UserProfiles",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_InstitutionId",
                table: "UserProfiles",
                column: "InstitutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_Classifiers_InstitutionId",
                table: "UserProfiles",
                column: "InstitutionId",
                principalTable: "Classifiers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_Classifiers_InstitutionId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_InstitutionId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ConfigurationInfo",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ProfileCreationDocumentDate",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ProfileCreationDocumentNumber",
                table: "UserProfiles");
        }
    }
}

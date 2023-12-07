using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveApplicationResourceUnusedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationResources_Classifiers_AssignedResourceSubTypeId",
                table: "ApplicationResources");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationResources_AssignedResourceSubTypeId",
                table: "ApplicationResources");

            migrationBuilder.DropColumn(
                name: "AssignedResourceDiffer",
                table: "ApplicationResources");

            migrationBuilder.DropColumn(
                name: "AssignedResourceSubTypeId",
                table: "ApplicationResources");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AssignedResourceDiffer",
                table: "ApplicationResources",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedResourceSubTypeId",
                table: "ApplicationResources",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResources_AssignedResourceSubTypeId",
                table: "ApplicationResources",
                column: "AssignedResourceSubTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationResources_Classifiers_AssignedResourceSubTypeId",
                table: "ApplicationResources",
                column: "AssignedResourceSubTypeId",
                principalTable: "Classifiers",
                principalColumn: "Id");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFieldsToApplicationResourceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnResourceDate",
                table: "ApplicationResources",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReturnResourceStateId",
                table: "ApplicationResources",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResources_ReturnResourceStateId",
                table: "ApplicationResources",
                column: "ReturnResourceStateId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationResources_Classifiers_ReturnResourceStateId",
                table: "ApplicationResources",
                column: "ReturnResourceStateId",
                principalTable: "Classifiers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationResources_Classifiers_ReturnResourceStateId",
                table: "ApplicationResources");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationResources_ReturnResourceStateId",
                table: "ApplicationResources");

            migrationBuilder.DropColumn(
                name: "ReturnResourceDate",
                table: "ApplicationResources");

            migrationBuilder.DropColumn(
                name: "ReturnResourceStateId",
                table: "ApplicationResources");
        }
    }
}

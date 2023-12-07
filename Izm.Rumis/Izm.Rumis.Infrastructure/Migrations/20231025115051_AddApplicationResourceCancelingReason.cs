using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationResourceCancelingReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelingDescription",
                table: "ApplicationResources",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "CancelingReasonId",
                table: "ApplicationResources",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationResources_CancelingReasonId",
                table: "ApplicationResources",
                column: "CancelingReasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationResources_Classifiers_CancelingReasonId",
                table: "ApplicationResources",
                column: "CancelingReasonId",
                principalTable: "Classifiers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationResources_Classifiers_CancelingReasonId",
                table: "ApplicationResources");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationResources_CancelingReasonId",
                table: "ApplicationResources");

            migrationBuilder.DropColumn(
                name: "CancelingDescription",
                table: "ApplicationResources");

            migrationBuilder.DropColumn(
                name: "CancelingReasonId",
                table: "ApplicationResources");

        }
    }
}

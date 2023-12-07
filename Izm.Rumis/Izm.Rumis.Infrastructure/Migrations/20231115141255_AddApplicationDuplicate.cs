using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationDuplicate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationDuplicateId",
                table: "Applications",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ApplicationDuplicateId",
                table: "Applications",
                column: "ApplicationDuplicateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Applications_ApplicationDuplicateId",
                table: "Applications",
                column: "ApplicationDuplicateId",
                principalTable: "Applications",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Applications_ApplicationDuplicateId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_ApplicationDuplicateId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ApplicationDuplicateId",
                table: "Applications");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    public partial class DefineFKForApplication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Applications_ResourceSubTypeId",
                table: "Applications",
                column: "ResourceSubTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Classifiers_ResourceSubTypeId",
                table: "Applications",
                column: "ResourceSubTypeId",
                principalTable: "Classifiers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Classifiers_ResourceSubTypeId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_ResourceSubTypeId",
                table: "Applications");
        }
    }
}

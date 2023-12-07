using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReworkPrivatePersonalIdentifiersForGdprAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrivatePersonalIdentifier",
                table: "GdprAudits",
                newName: "DataOwnerPrivatePersonalIdentifier");

            migrationBuilder.AddColumn<string>(
                name: "DataHandlerPrivatePersonalIdentifier",
                table: "GdprAudits",
                type: "varchar(11)",
                maxLength: 11,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GdprAudits_DataHandlerPrivatePersonalIdentifier",
                table: "GdprAudits",
                column: "DataHandlerPrivatePersonalIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_GdprAudits_DataOwnerPrivatePersonalIdentifier",
                table: "GdprAudits",
                column: "DataOwnerPrivatePersonalIdentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GdprAudits_DataHandlerPrivatePersonalIdentifier",
                table: "GdprAudits");

            migrationBuilder.DropIndex(
                name: "IX_GdprAudits_DataOwnerPrivatePersonalIdentifier",
                table: "GdprAudits");

            migrationBuilder.DropColumn(
                name: "DataHandlerPrivatePersonalIdentifier",
                table: "GdprAudits");

            migrationBuilder.RenameColumn(
                name: "DataOwnerPrivatePersonalIdentifier",
                table: "GdprAudits",
                newName: "PrivatePersonalIdentifier");
        }
    }
}

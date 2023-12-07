using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlterDataOwnerPpidInPersonDataReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrivatePersonlIdentifier",
                table: "PersonDataReports",
                newName: "DataOwnerPrivatePersonalIdentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DataOwnerPrivatePersonalIdentifier",
                table: "PersonDataReports",
                newName: "PrivatePersonlIdentifier");
        }
    }
}

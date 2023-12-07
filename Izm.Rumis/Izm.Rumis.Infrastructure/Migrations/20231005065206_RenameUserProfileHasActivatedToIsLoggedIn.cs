using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    public partial class RenameUserProfileHasActivatedToIsLoggedIn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HasActivated",
                table: "UserProfiles",
                newName: "IsLoggedIn");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsLoggedIn",
                table: "UserProfiles",
                newName: "HasActivated");
        }
    }
}

using Izm.Rumis.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    public partial class UpdateClassifierPermissionTypeEmptyFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"UPDATE Classifiers SET PermissionType = '{UserProfileType.Country}' WHERE PermissionType = ''");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

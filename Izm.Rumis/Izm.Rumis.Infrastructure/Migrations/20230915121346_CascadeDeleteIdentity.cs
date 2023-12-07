using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    public partial class CascadeDeleteIdentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityUserLogins_IdentityUsers_UserId",
                table: "IdentityUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_IdentityUserRoles_IdentityUsers_UserId",
                table: "IdentityUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_IdentityUsers_Users_Id",
                table: "IdentityUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUserLogins_IdentityUsers_UserId",
                table: "IdentityUserLogins",
                column: "UserId",
                principalTable: "IdentityUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUserRoles_IdentityUsers_UserId",
                table: "IdentityUserRoles",
                column: "UserId",
                principalTable: "IdentityUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUsers_Users_Id",
                table: "IdentityUsers",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityUserLogins_IdentityUsers_UserId",
                table: "IdentityUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_IdentityUserRoles_IdentityUsers_UserId",
                table: "IdentityUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_IdentityUsers_Users_Id",
                table: "IdentityUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUserLogins_IdentityUsers_UserId",
                table: "IdentityUserLogins",
                column: "UserId",
                principalTable: "IdentityUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUserRoles_IdentityUsers_UserId",
                table: "IdentityUserRoles",
                column: "UserId",
                principalTable: "IdentityUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUsers_Users_Id",
                table: "IdentityUsers",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}

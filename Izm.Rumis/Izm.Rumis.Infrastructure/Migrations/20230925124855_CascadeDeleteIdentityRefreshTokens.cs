using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    public partial class CascadeDeleteIdentityRefreshTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityRefreshTokens_IdentityUserLogins_UserLoginId",
                table: "IdentityRefreshTokens");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityRefreshTokens_IdentityUserLogins_UserLoginId",
                table: "IdentityRefreshTokens",
                column: "UserLoginId",
                principalTable: "IdentityUserLogins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityRefreshTokens_IdentityUserLogins_UserLoginId",
                table: "IdentityRefreshTokens");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityRefreshTokens_IdentityUserLogins_UserLoginId",
                table: "IdentityRefreshTokens",
                column: "UserLoginId",
                principalTable: "IdentityUserLogins",
                principalColumn: "Id");
        }
    }
}

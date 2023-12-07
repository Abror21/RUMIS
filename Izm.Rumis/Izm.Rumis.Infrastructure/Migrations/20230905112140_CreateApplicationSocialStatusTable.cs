using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    public partial class CreateApplicationSocialStatusTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationSocialStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ApplicationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SocialStatusId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ModifiedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationSocialStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationSocialStatuses_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationSocialStatuses_Classifiers_SocialStatusId",
                        column: x => x.SocialStatusId,
                        principalTable: "Classifiers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationSocialStatuses_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationSocialStatuses_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSocialStatuses_ApplicationId",
                table: "ApplicationSocialStatuses",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSocialStatuses_CreatedById",
                table: "ApplicationSocialStatuses",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSocialStatuses_ModifiedById",
                table: "ApplicationSocialStatuses",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSocialStatuses_SocialStatusId",
                table: "ApplicationSocialStatuses",
                column: "SocialStatusId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationSocialStatuses");
        }
    }
}

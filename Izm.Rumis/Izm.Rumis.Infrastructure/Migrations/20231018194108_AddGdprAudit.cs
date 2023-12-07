using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGdprAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GdprAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UnitOfWorkId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    PrivatePersonalIdentifier = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserProfileId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DataHandlerId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Action = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActionData = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataOwnerId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GdprAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GdprAudits_PersonTechnicals_DataHandlerId",
                        column: x => x.DataHandlerId,
                        principalTable: "PersonTechnicals",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GdprAudits_PersonTechnicals_DataOwnerId",
                        column: x => x.DataOwnerId,
                        principalTable: "PersonTechnicals",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GdprAudits_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GdprAudits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GdprAuditData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GdprAuditId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GdprAuditData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GdprAuditData_GdprAudits_GdprAuditId",
                        column: x => x.GdprAuditId,
                        principalTable: "GdprAudits",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GdprAuditData_GdprAuditId",
                table: "GdprAuditData",
                column: "GdprAuditId");

            migrationBuilder.CreateIndex(
                name: "IX_GdprAudits_DataHandlerId",
                table: "GdprAudits",
                column: "DataHandlerId");

            migrationBuilder.CreateIndex(
                name: "IX_GdprAudits_DataOwnerId",
                table: "GdprAudits",
                column: "DataOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_GdprAudits_UserId",
                table: "GdprAudits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GdprAudits_UserProfileId",
                table: "GdprAudits",
                column: "UserProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GdprAuditData");

            migrationBuilder.DropTable(
                name: "GdprAudits");
        }
    }
}

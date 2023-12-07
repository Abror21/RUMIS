using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationMonitoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MonitoringClassGrade",
                table: "Applications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MonitoringClassParallel",
                table: "Applications",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "MonitoringEducationalStatusId",
                table: "Applications",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MonitoringEducationalSubStatusId",
                table: "Applications",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "MonitoringGroup",
                table: "Applications",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "MonitoringWorkStatusId",
                table: "Applications",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_MonitoringEducationalStatusId",
                table: "Applications",
                column: "MonitoringEducationalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_MonitoringEducationalSubStatusId",
                table: "Applications",
                column: "MonitoringEducationalSubStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_MonitoringWorkStatusId",
                table: "Applications",
                column: "MonitoringWorkStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Classifiers_MonitoringEducationalStatusId",
                table: "Applications",
                column: "MonitoringEducationalStatusId",
                principalTable: "Classifiers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Classifiers_MonitoringEducationalSubStatusId",
                table: "Applications",
                column: "MonitoringEducationalSubStatusId",
                principalTable: "Classifiers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Classifiers_MonitoringWorkStatusId",
                table: "Applications",
                column: "MonitoringWorkStatusId",
                principalTable: "Classifiers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Classifiers_MonitoringEducationalStatusId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Classifiers_MonitoringEducationalSubStatusId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Classifiers_MonitoringWorkStatusId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_MonitoringEducationalStatusId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_MonitoringEducationalSubStatusId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_MonitoringWorkStatusId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "MonitoringClassGrade",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "MonitoringClassParallel",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "MonitoringEducationalStatusId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "MonitoringEducationalSubStatusId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "MonitoringGroup",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "MonitoringWorkStatusId",
                table: "Applications");
        }
    }
}

using System;
using Izm.Rumis.Infrastructure.Common;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialSupportResource : Migration
    {
        private Guid ResourceSubTypeId = Guid.Parse("6edb243c-efb9-42f0-bda0-51d8becc208c");
        private Guid ResourceStatusId = Guid.Parse("214a7d61-e368-4824-ab01-e96bca991e60");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var dateTimeValue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            migrationBuilder.Sql($"INSERT IGNORE INTO Classifiers VALUES ('{ResourceSubTypeId}', 'resource_subtype', 'windows_laptop', 'Windows portatīvais dators', NULL, NULL, NULL, NULL, 0, 0, '{dateTimeValue}', '{dateTimeValue}', '{UserIds.Application}', '{UserIds.Application}', NULL, 'Country', NULL);");

            migrationBuilder.Sql($"INSERT IGNORE INTO Classifiers VALUES ('{ResourceStatusId}', 'resource_status', 'new', 'Jauns', NULL, NULL, NULL, NULL, 0, 0, '{dateTimeValue}', '{dateTimeValue}', '{UserIds.Application}', '{UserIds.Application}', NULL, 'Country', NULL);");

            migrationBuilder.AlterColumn<Guid>(
                name: "ResourceSubTypeId",
                table: "Resources",
                type: "char(36)",
                nullable: false,
                defaultValue: ResourceSubTypeId,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "ResourceStatusId",
                table: "Resources",
                type: "char(36)",
                nullable: false,
                defaultValue: ResourceStatusId,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "SocialSupportResource",
                table: "Resources",
                type: "tinyint(1)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SocialSupportResource",
                table: "Resources");

            migrationBuilder.AlterColumn<Guid>(
                name: "ResourceSubTypeId",
                table: "Resources",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "ResourceStatusId",
                table: "Resources",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");
        }
    }
}

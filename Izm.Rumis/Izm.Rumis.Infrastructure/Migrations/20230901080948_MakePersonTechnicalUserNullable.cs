using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    public partial class MakePersonTechnicalUserNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonContact_Classifiers_ContactTypeId",
                table: "PersonContact");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonContact_PersonTechnicals_PersonTechnicalId",
                table: "PersonContact");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonContact_Users_CreatedById",
                table: "PersonContact");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonContact_Users_ModifiedById",
                table: "PersonContact");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonTechnicals_Users_UserId",
                table: "PersonTechnicals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonContact",
                table: "PersonContact");

            migrationBuilder.RenameTable(
                name: "PersonContact",
                newName: "PersonContacts");

            migrationBuilder.RenameIndex(
                name: "IX_PersonContact_PersonTechnicalId",
                table: "PersonContacts",
                newName: "IX_PersonContacts_PersonTechnicalId");

            migrationBuilder.RenameIndex(
                name: "IX_PersonContact_ModifiedById",
                table: "PersonContacts",
                newName: "IX_PersonContacts_ModifiedById");

            migrationBuilder.RenameIndex(
                name: "IX_PersonContact_CreatedById",
                table: "PersonContacts",
                newName: "IX_PersonContacts_CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_PersonContact_ContactTypeId",
                table: "PersonContacts",
                newName: "IX_PersonContacts_ContactTypeId");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "PersonTechnicals",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonContacts",
                table: "PersonContacts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContacts_Classifiers_ContactTypeId",
                table: "PersonContacts",
                column: "ContactTypeId",
                principalTable: "Classifiers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContacts_PersonTechnicals_PersonTechnicalId",
                table: "PersonContacts",
                column: "PersonTechnicalId",
                principalTable: "PersonTechnicals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContacts_Users_CreatedById",
                table: "PersonContacts",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContacts_Users_ModifiedById",
                table: "PersonContacts",
                column: "ModifiedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonTechnicals_Users_UserId",
                table: "PersonTechnicals",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonContacts_Classifiers_ContactTypeId",
                table: "PersonContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonContacts_PersonTechnicals_PersonTechnicalId",
                table: "PersonContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonContacts_Users_CreatedById",
                table: "PersonContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonContacts_Users_ModifiedById",
                table: "PersonContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonTechnicals_Users_UserId",
                table: "PersonTechnicals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonContacts",
                table: "PersonContacts");

            migrationBuilder.RenameTable(
                name: "PersonContacts",
                newName: "PersonContact");

            migrationBuilder.RenameIndex(
                name: "IX_PersonContacts_PersonTechnicalId",
                table: "PersonContact",
                newName: "IX_PersonContact_PersonTechnicalId");

            migrationBuilder.RenameIndex(
                name: "IX_PersonContacts_ModifiedById",
                table: "PersonContact",
                newName: "IX_PersonContact_ModifiedById");

            migrationBuilder.RenameIndex(
                name: "IX_PersonContacts_CreatedById",
                table: "PersonContact",
                newName: "IX_PersonContact_CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_PersonContacts_ContactTypeId",
                table: "PersonContact",
                newName: "IX_PersonContact_ContactTypeId");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "PersonTechnicals",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonContact",
                table: "PersonContact",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContact_Classifiers_ContactTypeId",
                table: "PersonContact",
                column: "ContactTypeId",
                principalTable: "Classifiers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContact_PersonTechnicals_PersonTechnicalId",
                table: "PersonContact",
                column: "PersonTechnicalId",
                principalTable: "PersonTechnicals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContact_Users_CreatedById",
                table: "PersonContact",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContact_Users_ModifiedById",
                table: "PersonContact",
                column: "ModifiedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonTechnicals_Users_UserId",
                table: "PersonTechnicals",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

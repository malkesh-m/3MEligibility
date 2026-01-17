using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EligibilityPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTable_User : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Entity_EntityId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_EntityId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ForcePasswordChange",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastPasswordUpdate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NoOfTrials",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ResetPasswordExpires",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ResetPasswordToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedByDateTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserPassword",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserPicture",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "issuspended",
                table: "Users",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "SuspentionDate",
                table: "Users",
                newName: "LastLoginAt");

            migrationBuilder.RenameColumn(
                name: "LoginId",
                table: "Users",
                newName: "KeycloakUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_LoginId",
                table: "Users",
                newName: "IX_Users_KeycloakUserId");

            migrationBuilder.AddColumn<DateOnly>(
                name: "CreatedAt",
                table: "Users",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "LastLoginAt",
                table: "Users",
                newName: "SuspentionDate");

            migrationBuilder.RenameColumn(
                name: "KeycloakUserId",
                table: "Users",
                newName: "LoginId");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Users",
                newName: "issuspended");

            migrationBuilder.RenameIndex(
                name: "IX_Users_KeycloakUserId",
                table: "Users",
                newName: "IX_Users_LoginId");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateOnly>(
                name: "CreationDate",
                table: "Users",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ForcePasswordChange",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginDate",
                table: "Users",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPasswordUpdate",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "Users",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "NoOfTrials",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetPasswordExpires",
                table: "Users",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResetPasswordToken",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedByDateTime",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AddColumn<string>(
                name: "UserPassword",
                table: "Users",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<byte[]>(
                name: "UserPicture",
                table: "Users",
                type: "longblob",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EntityId",
                table: "Users",
                column: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Entity_EntityId",
                table: "Users",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

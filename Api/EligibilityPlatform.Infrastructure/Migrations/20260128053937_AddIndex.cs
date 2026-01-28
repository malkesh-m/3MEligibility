using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MEligibilityPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_GroupRoles_GroupId",
                table: "GroupRoles",
                newName: "IX_GroupRole_GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroup_UserId",
                table: "UserGroups",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_RoleId",
                table: "Roles",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserGroup_UserId",
                table: "UserGroups");

            migrationBuilder.DropIndex(
                name: "IX_Role_RoleId",
                table: "Roles");

            migrationBuilder.RenameIndex(
                name: "IX_GroupRole_GroupId",
                table: "GroupRoles",
                newName: "IX_GroupRoles_GroupId");
        }
    }
}

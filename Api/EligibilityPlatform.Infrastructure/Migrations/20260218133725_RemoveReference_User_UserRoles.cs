using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MEligibilityPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReference_User_UserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__UserRole__UserI__40F9A68C",
                table: "UserRoles");

         

            migrationBuilder.CreateIndex(
                name: "IX_SecurityRoleUser_UsersUserId",
                table: "SecurityRoleUser",
                column: "UsersUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles");

       

            migrationBuilder.AddForeignKey(
                name: "FK__UserRole__UserI__40F9A68C",
                table: "UserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}

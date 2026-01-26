using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MEligibilityPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserReference_UserGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__UserGroup__UserI__40F9A68C",
                table: "UserGroups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK__UserGroup__UserI__40F9A68C",
                table: "UserGroups",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}

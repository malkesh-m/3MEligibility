using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MEligibilityPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantId_UserGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "UserGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "GroupRoles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "GroupRoles");
        }
    }
}

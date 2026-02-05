using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MEligibilityPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnApiId_ApiParameterMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApiId",
                table: "ApiParameterMaps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ApiParameterMaps_ApiId",
                table: "ApiParameterMaps",
                column: "ApiId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiParameterMaps_NodeAPIs_ApiId",
                table: "ApiParameterMaps",
                column: "ApiId",
                principalTable: "NodeAPIs",
                principalColumn: "APIId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiParameterMaps_NodeAPIs_ApiId",
                table: "ApiParameterMaps");

            migrationBuilder.DropIndex(
                name: "IX_ApiParameterMaps_ApiId",
                table: "ApiParameterMaps");

            migrationBuilder.DropColumn(
                name: "ApiId",
                table: "ApiParameterMaps");
        }
    }
}

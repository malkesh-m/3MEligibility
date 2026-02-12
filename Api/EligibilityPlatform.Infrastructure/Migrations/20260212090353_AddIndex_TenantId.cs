using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MEligibilityPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndex_TenantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserGroup_TenantId",
                table: "UserGroups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityGroup_TenantId",
                table: "SecurityGroups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCapAmount_TenantId",
                table: "ProductCapAmount",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCap_TenantId",
                table: "ProductCap",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterBinding_TenantId",
                table: "ParameterBinding",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "NodeAPIs.IX_NodeAPIs_TenantId",
                table: "NodeAPIs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ListItem_TenantId",
                table: "ListItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupRole_TenantId",
                table: "GroupRoles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationHistory_TenantId",
                table: "EvaluationHistories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "Audit_TenantId",
                table: "Audit",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiParameter_TenantId",
                table: "ApiParameters",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiParameterMap_TenantId",
                table: "ApiParameterMaps",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserGroup_TenantId",
                table: "UserGroups");

            migrationBuilder.DropIndex(
                name: "IX_SecurityGroup_TenantId",
                table: "SecurityGroups");

            migrationBuilder.DropIndex(
                name: "IX_ProductCapAmount_TenantId",
                table: "ProductCapAmount");

            migrationBuilder.DropIndex(
                name: "IX_ProductCap_TenantId",
                table: "ProductCap");

            migrationBuilder.DropIndex(
                name: "IX_ParameterBinding_TenantId",
                table: "ParameterBinding");

            migrationBuilder.DropIndex(
                name: "NodeAPIs.IX_NodeAPIs_TenantId",
                table: "NodeAPIs");

            migrationBuilder.DropIndex(
                name: "IX_ListItem_TenantId",
                table: "ListItems");

            migrationBuilder.DropIndex(
                name: "IX_GroupRole_TenantId",
                table: "GroupRoles");

            migrationBuilder.DropIndex(
                name: "IX_EvaluationHistory_TenantId",
                table: "EvaluationHistories");

            migrationBuilder.DropIndex(
                name: "Audit_TenantId",
                table: "Audit");

            migrationBuilder.DropIndex(
                name: "IX_ApiParameter_TenantId",
                table: "ApiParameters");

            migrationBuilder.DropIndex(
                name: "IX_ApiParameterMap_TenantId",
                table: "ApiParameterMaps");
        }
    }
}

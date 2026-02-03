using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MEligibilityPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnTenantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "SecurityGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ProductCapAmount",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ProductCap",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "NodeAPIs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "MakerChecker",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ListItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "EvaluationHistories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Audit",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ApiParameters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ApiParameterMaps",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SecurityGroups");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductCapAmount");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductCap");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "NodeAPIs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "MakerChecker");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ListItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Audit");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ApiParameters");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ApiParameterMaps");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "EvaluationHistories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}

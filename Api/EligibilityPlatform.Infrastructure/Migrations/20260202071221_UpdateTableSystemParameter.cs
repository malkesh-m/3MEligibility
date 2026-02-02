using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MEligibilityPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableSystemParameter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SourceParameters");

            migrationBuilder.DropColumn(
                name: "SystemParameter",
                table: "ParameterBinding");

            migrationBuilder.AddColumn<int>(
                name: "SystemParameterId",
                table: "ParameterBinding",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SystemParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemParameters", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterBinding_SystemParameterId",
                table: "ParameterBinding",
                column: "SystemParameterId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParameterBinding_SystemParameters_SystemParameterId",
                table: "ParameterBinding",
                column: "SystemParameterId",
                principalTable: "SystemParameters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParameterBinding_SystemParameters_SystemParameterId",
                table: "ParameterBinding");

            migrationBuilder.DropTable(
                name: "SystemParameters");

            migrationBuilder.DropIndex(
                name: "IX_ParameterBinding_SystemParameterId",
                table: "ParameterBinding");

            migrationBuilder.DropColumn(
                name: "SystemParameterId",
                table: "ParameterBinding");

            migrationBuilder.AddColumn<string>(
                name: "SystemParameter",
                table: "ParameterBinding",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SourceParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceParameters", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}

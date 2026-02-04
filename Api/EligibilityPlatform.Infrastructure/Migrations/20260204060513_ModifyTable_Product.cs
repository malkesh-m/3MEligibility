using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MEligibilityPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifyTable_Product : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductImage",
                table: "Product");

            migrationBuilder.AddColumn<string>(
                name: "ProductImagePath",
                table: "Product",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductImagePath",
                table: "Product");

            migrationBuilder.AddColumn<byte[]>(
                name: "ProductImage",
                table: "Product",
                type: "longblob",
                nullable: true);
        }
    }
}

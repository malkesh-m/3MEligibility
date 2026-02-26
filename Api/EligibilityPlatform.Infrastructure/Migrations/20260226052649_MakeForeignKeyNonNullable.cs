using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MEligibilityPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeForeignKeyNonNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiParameters_NodeAPIs_ApiId",
                table: "ApiParameters");

            migrationBuilder.DropForeignKey(
                name: "FK__Factors__Paramet__1EA48E88",
                table: "Factors");

            migrationBuilder.DropForeignKey(
                name: "FK_IntegrationApiEvaluation_EvaluationHistories_EvaluationHisto~",
                table: "IntegrationApiEvaluation");

            migrationBuilder.DropForeignKey(
                name: "FK__NodeAPIs__NodeId__2EDAF651",
                table: "NodeAPIs");

            migrationBuilder.DropForeignKey(
                name: "FK__PCards__ProductI__3A4CA8FD",
                table: "PCards");

            migrationBuilder.DropForeignKey(
                name: "FK__Product__Categor__3B40CD36",
                table: "Product");

            migrationBuilder.UpdateData(
                table: "SecurityRoles",
                keyColumn: "RoleName",
                keyValue: null,
                column: "RoleName",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "RoleName",
                table: "SecurityRoles",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "ProductCapAmount",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Product",
                keyColumn: "Code",
                keyValue: null,
                column: "Code",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Product",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "PCards",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Parameter",
                keyColumn: "ParameterName",
                keyValue: null,
                column: "ParameterName",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ParameterName",
                table: "Parameter",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Nodes",
                keyColumn: "NodeURL",
                keyValue: null,
                column: "NodeURL",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "NodeURL",
                table: "Nodes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "NodeId",
                table: "NodeAPIs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "NodeAPIs",
                keyColumn: "CreatedBy",
                keyValue: null,
                column: "CreatedBy",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "NodeAPIs",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "NodeAPIs",
                keyColumn: "APIName",
                keyValue: null,
                column: "APIName",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "APIName",
                table: "NodeAPIs",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "ManagedList",
                keyColumn: "ListName",
                keyValue: null,
                column: "ListName",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ListName",
                table: "ManagedList",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "EvaluationHistoryId",
                table: "IntegrationApiEvaluation",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ParameterId",
                table: "Factors",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Factors",
                keyColumn: "FactorName",
                keyValue: null,
                column: "FactorName",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "FactorName",
                table: "Factors",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Audit",
                keyColumn: "UserName",
                keyValue: null,
                column: "UserName",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "Audit",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "ApiId",
                table: "ApiParameters",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ApiParameters_NodeAPIs_ApiId",
                table: "ApiParameters",
                column: "ApiId",
                principalTable: "NodeAPIs",
                principalColumn: "APIId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__Factors__Paramet__1EA48E88",
                table: "Factors",
                column: "ParameterId",
                principalTable: "Parameter",
                principalColumn: "ParameterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IntegrationApiEvaluation_EvaluationHistories_EvaluationHisto~",
                table: "IntegrationApiEvaluation",
                column: "EvaluationHistoryId",
                principalTable: "EvaluationHistories",
                principalColumn: "EvaluationHistoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__NodeAPIs__NodeId__2EDAF651",
                table: "NodeAPIs",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "NodeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__PCards__ProductI__3A4CA8FD",
                table: "PCards",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__Product__Categor__3B40CD36",
                table: "Product",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiParameters_NodeAPIs_ApiId",
                table: "ApiParameters");

            migrationBuilder.DropForeignKey(
                name: "FK__Factors__Paramet__1EA48E88",
                table: "Factors");

            migrationBuilder.DropForeignKey(
                name: "FK_IntegrationApiEvaluation_EvaluationHistories_EvaluationHisto~",
                table: "IntegrationApiEvaluation");

            migrationBuilder.DropForeignKey(
                name: "FK__NodeAPIs__NodeId__2EDAF651",
                table: "NodeAPIs");

            migrationBuilder.DropForeignKey(
                name: "FK__PCards__ProductI__3A4CA8FD",
                table: "PCards");

            migrationBuilder.DropForeignKey(
                name: "FK__Product__Categor__3B40CD36",
                table: "Product");

            migrationBuilder.AlterColumn<string>(
                name: "RoleName",
                table: "SecurityRoles",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "ProductCapAmount",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Product",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Product",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "PCards",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ParameterName",
                table: "Parameter",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "NodeURL",
                table: "Nodes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "NodeId",
                table: "NodeAPIs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "NodeAPIs",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "APIName",
                table: "NodeAPIs",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ListName",
                table: "ManagedList",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "EvaluationHistoryId",
                table: "IntegrationApiEvaluation",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ParameterId",
                table: "Factors",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "FactorName",
                table: "Factors",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "Audit",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "ApiId",
                table: "ApiParameters",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiParameters_NodeAPIs_ApiId",
                table: "ApiParameters",
                column: "ApiId",
                principalTable: "NodeAPIs",
                principalColumn: "APIId");

            migrationBuilder.AddForeignKey(
                name: "FK__Factors__Paramet__1EA48E88",
                table: "Factors",
                column: "ParameterId",
                principalTable: "Parameter",
                principalColumn: "ParameterId");

            migrationBuilder.AddForeignKey(
                name: "FK_IntegrationApiEvaluation_EvaluationHistories_EvaluationHisto~",
                table: "IntegrationApiEvaluation",
                column: "EvaluationHistoryId",
                principalTable: "EvaluationHistories",
                principalColumn: "EvaluationHistoryId");

            migrationBuilder.AddForeignKey(
                name: "FK__NodeAPIs__NodeId__2EDAF651",
                table: "NodeAPIs",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "NodeId");

            migrationBuilder.AddForeignKey(
                name: "FK__PCards__ProductI__3A4CA8FD",
                table: "PCards",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK__Product__Categor__3B40CD36",
                table: "Product",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "CategoryId");
        }
    }
}

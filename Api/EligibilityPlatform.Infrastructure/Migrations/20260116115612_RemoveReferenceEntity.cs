using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EligibilityPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReferenceEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Category__Entity__123EB7A3",
                table: "Category");

            migrationBuilder.DropForeignKey(
                name: "FK__ECards__EntityId__160F4887",
                table: "ECards");

            migrationBuilder.DropForeignKey(
                name: "FK__Entity__CityId__300424B4",
                table: "Entity");

            migrationBuilder.DropForeignKey(
                name: "FK__Entity__CountryI__2F10007B",
                table: "Entity");

            migrationBuilder.DropForeignKey(
                name: "FK__ERule__EntityId__19DFD96B",
                table: "ERule");

            migrationBuilder.DropForeignKey(
                name: "FK_EruleMaster_Entity_EntityId",
                table: "EruleMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationHistories_Entity_EntityId",
                table: "EvaluationHistories");

            migrationBuilder.DropForeignKey(
                name: "FK__ExceptionMgmt__EntityId__20DFD973",
                table: "ExceptionManagements");

            migrationBuilder.DropForeignKey(
                name: "FK__Factors__EntityI__1DB06A4F",
                table: "Factors");

            migrationBuilder.DropForeignKey(
                name: "FK__HistoryPC__Entit__29221CFB",
                table: "HistoryPC");

            migrationBuilder.DropForeignKey(
                name: "FK__ManagedLi__Entit__2DE6D218",
                table: "ManagedList");

            migrationBuilder.DropForeignKey(
                name: "FK__Nodes__EntityId__2FCF1A8A",
                table: "Nodes");

            migrationBuilder.DropForeignKey(
                name: "FK__Parameter__Entit__32AB8735",
                table: "Parameter");

            migrationBuilder.DropForeignKey(
                name: "FK__PCards__EntityId__395884C4",
                table: "PCards");

            migrationBuilder.DropForeignKey(
                name: "FK__Product__EntityI__3C34F16F",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Settings_Entity_EntityId",
                table: "Settings");

            migrationBuilder.DropForeignKey(
                name: "FK__Users__EntityId__41EDCAC5",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "UQ_Parameter_EntityId_ParameterName",
                table: "Parameter");

            migrationBuilder.DropIndex(
                name: "IX_EvaluationHistories_EntityId",
                table: "EvaluationHistories");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "ProductParam",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductParam_EntityId",
                table: "ProductParam",
                newName: "IX_ProductParam_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_ExceptionManagement_EntityId1",
                table: "ExceptionManagements",
                newName: "IX_ExceptionManagements_EntityId");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "EvaluationHistories",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "EruleMaster",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_ExceptionManagement_EntityId",
                table: "EruleMaster",
                newName: "IX_ExceptionManagement_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_EruleMaster_EruleName_EntityId",
                table: "EruleMaster",
                newName: "IX_EruleMaster_EruleName_TenantId");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "Settings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Settings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "Product",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "PCards",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "PCards",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "Parameter",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Parameter",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "Nodes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Nodes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "ManagedList",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ManagedList",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "HistoryPC",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "HistoryPC",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "Factors",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Factors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "ExceptionManagements",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ExceptionManagements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "ERule",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ERule",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "ECards",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ECards",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "Category",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Category",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_TenantId",
                table: "Settings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_TenantId",
                table: "Product",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PCards_TenantId",
                table: "PCards",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Parameter_TenantId",
                table: "Parameter",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "UQ_Parameter_TenantId_ParameterName",
                table: "Parameter",
                columns: new[] { "TenantId", "ParameterName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_TenantId",
                table: "Nodes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagedList_TenantId",
                table: "ManagedList",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryPC_TenantId",
                table: "HistoryPC",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Factors_TenantId",
                table: "Factors",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionManagement_TenantId1",
                table: "ExceptionManagements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ERule_TenantId",
                table: "ERule",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ECards_TenantId",
                table: "ECards",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Category_TenantId",
                table: "Category",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Category_Entity_EntityId",
                table: "Category",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_ECards_Entity_EntityId",
                table: "ECards",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entity_City_CityId",
                table: "Entity",
                column: "CityId",
                principalTable: "City",
                principalColumn: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entity_Country_CountryId",
                table: "Entity",
                column: "CountryId",
                principalTable: "Country",
                principalColumn: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ERule_Entity_EntityId",
                table: "ERule",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExceptionManagements_Entity_EntityId",
                table: "ExceptionManagements",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Factors_Entity_EntityId",
                table: "Factors",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_HistoryPC_Entity_EntityId",
                table: "HistoryPC",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_ManagedList_Entity_EntityId",
                table: "ManagedList",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_Entity_EntityId",
                table: "Nodes",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parameter_Entity_EntityId",
                table: "Parameter",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_PCards_Entity_EntityId",
                table: "PCards",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Entity_EntityId",
                table: "Product",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Settings_Entity_EntityId",
                table: "Settings",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Entity_EntityId",
                table: "Users",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Category_Entity_EntityId",
                table: "Category");

            migrationBuilder.DropForeignKey(
                name: "FK_ECards_Entity_EntityId",
                table: "ECards");

            migrationBuilder.DropForeignKey(
                name: "FK_Entity_City_CityId",
                table: "Entity");

            migrationBuilder.DropForeignKey(
                name: "FK_Entity_Country_CountryId",
                table: "Entity");

            migrationBuilder.DropForeignKey(
                name: "FK_ERule_Entity_EntityId",
                table: "ERule");

            migrationBuilder.DropForeignKey(
                name: "FK_ExceptionManagements_Entity_EntityId",
                table: "ExceptionManagements");

            migrationBuilder.DropForeignKey(
                name: "FK_Factors_Entity_EntityId",
                table: "Factors");

            migrationBuilder.DropForeignKey(
                name: "FK_HistoryPC_Entity_EntityId",
                table: "HistoryPC");

            migrationBuilder.DropForeignKey(
                name: "FK_ManagedList_Entity_EntityId",
                table: "ManagedList");

            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_Entity_EntityId",
                table: "Nodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Parameter_Entity_EntityId",
                table: "Parameter");

            migrationBuilder.DropForeignKey(
                name: "FK_PCards_Entity_EntityId",
                table: "PCards");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Entity_EntityId",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Settings_Entity_EntityId",
                table: "Settings");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Entity_EntityId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Settings_TenantId",
                table: "Settings");

            migrationBuilder.DropIndex(
                name: "IX_Product_TenantId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_PCards_TenantId",
                table: "PCards");

            migrationBuilder.DropIndex(
                name: "IX_Parameter_TenantId",
                table: "Parameter");

            migrationBuilder.DropIndex(
                name: "UQ_Parameter_TenantId_ParameterName",
                table: "Parameter");

            migrationBuilder.DropIndex(
                name: "IX_Nodes_TenantId",
                table: "Nodes");

            migrationBuilder.DropIndex(
                name: "IX_ManagedList_TenantId",
                table: "ManagedList");

            migrationBuilder.DropIndex(
                name: "IX_HistoryPC_TenantId",
                table: "HistoryPC");

            migrationBuilder.DropIndex(
                name: "IX_Factors_TenantId",
                table: "Factors");

            migrationBuilder.DropIndex(
                name: "IX_ExceptionManagement_TenantId1",
                table: "ExceptionManagements");

            migrationBuilder.DropIndex(
                name: "IX_ERule_TenantId",
                table: "ERule");

            migrationBuilder.DropIndex(
                name: "IX_ECards_TenantId",
                table: "ECards");

            migrationBuilder.DropIndex(
                name: "IX_Category_TenantId",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PCards");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Parameter");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ManagedList");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "HistoryPC");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Factors");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ExceptionManagements");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ERule");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ECards");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Category");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "ProductParam",
                newName: "EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductParam_TenantId",
                table: "ProductParam",
                newName: "IX_ProductParam_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_ExceptionManagements_EntityId",
                table: "ExceptionManagements",
                newName: "IX_ExceptionManagement_EntityId1");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "EvaluationHistories",
                newName: "EntityId");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "EruleMaster",
                newName: "EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_ExceptionManagement_TenantId",
                table: "EruleMaster",
                newName: "IX_ExceptionManagement_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_EruleMaster_EruleName_TenantId",
                table: "EruleMaster",
                newName: "IX_EruleMaster_EruleName_EntityId");

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "Settings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "PCards",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "Parameter",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "Nodes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "ManagedList",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "HistoryPC",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "Factors",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "ExceptionManagements",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "ERule",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "ECards",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "Category",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Parameter_EntityId_ParameterName",
                table: "Parameter",
                columns: new[] { "EntityId", "ParameterName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationHistories_EntityId",
                table: "EvaluationHistories",
                column: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK__Category__Entity__123EB7A3",
                table: "Category",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__ECards__EntityId__160F4887",
                table: "ECards",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__Entity__CityId__300424B4",
                table: "Entity",
                column: "CityId",
                principalTable: "City",
                principalColumn: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK__Entity__CountryI__2F10007B",
                table: "Entity",
                column: "CountryId",
                principalTable: "Country",
                principalColumn: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK__ERule__EntityId__19DFD96B",
                table: "ERule",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EruleMaster_Entity_EntityId",
                table: "EruleMaster",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationHistories_Entity_EntityId",
                table: "EvaluationHistories",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK__ExceptionMgmt__EntityId__20DFD973",
                table: "ExceptionManagements",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__Factors__EntityI__1DB06A4F",
                table: "Factors",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__HistoryPC__Entit__29221CFB",
                table: "HistoryPC",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__ManagedLi__Entit__2DE6D218",
                table: "ManagedList",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__Nodes__EntityId__2FCF1A8A",
                table: "Nodes",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__Parameter__Entit__32AB8735",
                table: "Parameter",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__PCards__EntityId__395884C4",
                table: "PCards",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__Product__EntityI__3C34F16F",
                table: "Product",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Settings_Entity_EntityId",
                table: "Settings",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__Users__EntityId__41EDCAC5",
                table: "Users",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

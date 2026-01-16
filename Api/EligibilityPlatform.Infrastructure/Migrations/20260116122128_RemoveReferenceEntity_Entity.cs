using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EligibilityPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReferenceEntity_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropIndex(
                name: "IX_Settings_EntityId",
                table: "Settings");

            migrationBuilder.DropIndex(
                name: "IX_Product_EntityId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_PCards_EntityId",
                table: "PCards");

            migrationBuilder.DropIndex(
                name: "IX_Parameter_EntityId",
                table: "Parameter");

            migrationBuilder.DropIndex(
                name: "IX_Nodes_EntityId",
                table: "Nodes");

            migrationBuilder.DropIndex(
                name: "IX_ManagedList_EntityId",
                table: "ManagedList");

            migrationBuilder.DropIndex(
                name: "IX_HistoryPC_EntityId",
                table: "HistoryPC");

            migrationBuilder.DropIndex(
                name: "IX_Factors_EntityId",
                table: "Factors");

            migrationBuilder.DropIndex(
                name: "IX_ExceptionManagements_EntityId",
                table: "ExceptionManagements");

            migrationBuilder.DropIndex(
                name: "IX_ERule_EntityId",
                table: "ERule");

            migrationBuilder.DropIndex(
                name: "IX_Entity_CityId",
                table: "Entity");

            migrationBuilder.DropIndex(
                name: "IX_Entity_CountryId",
                table: "Entity");

            migrationBuilder.DropIndex(
                name: "IX_ECards_EntityId",
                table: "ECards");

            migrationBuilder.DropIndex(
                name: "IX_Category_EntityId",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "PCards");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Parameter");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "ManagedList");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "HistoryPC");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Factors");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "ExceptionManagements");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "ERule");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "ECards");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "Settings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "Product",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "PCards",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "Parameter",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "Nodes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "ManagedList",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "HistoryPC",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "Factors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "ExceptionManagements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "ERule",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "ECards",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "Category",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Settings_EntityId",
                table: "Settings",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_EntityId",
                table: "Product",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PCards_EntityId",
                table: "PCards",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Parameter_EntityId",
                table: "Parameter",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_EntityId",
                table: "Nodes",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagedList_EntityId",
                table: "ManagedList",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryPC_EntityId",
                table: "HistoryPC",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Factors_EntityId",
                table: "Factors",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionManagements_EntityId",
                table: "ExceptionManagements",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ERule_EntityId",
                table: "ERule",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Entity_CityId",
                table: "Entity",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Entity_CountryId",
                table: "Entity",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_ECards_EntityId",
                table: "ECards",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Category_EntityId",
                table: "Category",
                column: "EntityId");

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
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBSCardManager.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogCompatibilityMetadataV201 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CatalogConfidence",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CatalogRecordId",
                table: "Products",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CatalogSource",
                table: "Products",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CatalogSourceRecordId",
                table: "Products",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CatalogUpdatedAt",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CatalogVersion",
                table: "Products",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeprecated",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUserCreated",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ReplacedByCatalogRecordId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CatalogConfidence",
                table: "ChecklistItems",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CatalogRecordId",
                table: "ChecklistItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CatalogSource",
                table: "ChecklistItems",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CatalogSourceRecordId",
                table: "ChecklistItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CatalogUpdatedAt",
                table: "ChecklistItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CatalogVersion",
                table: "ChecklistItems",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeprecated",
                table: "ChecklistItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUserCreated",
                table: "ChecklistItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "ChecklistItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ReplacedByCatalogRecordId",
                table: "ChecklistItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CatalogConfidence",
                table: "Cards",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CatalogRecordId",
                table: "Cards",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CatalogSource",
                table: "Cards",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CatalogSourceRecordId",
                table: "Cards",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CatalogUpdatedAt",
                table: "Cards",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CatalogVersion",
                table: "Cards",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeprecated",
                table: "Cards",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUserCreated",
                table: "Cards",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Cards",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ReplacedByCatalogRecordId",
                table: "Cards",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CatalogRecordId",
                table: "Products",
                column: "CatalogRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CatalogSourceRecordId_CatalogVersion_IsVerified",
                table: "Products",
                columns: new[] { "CatalogSourceRecordId", "CatalogVersion", "IsVerified" });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItems_CatalogRecordId",
                table: "ChecklistItems",
                column: "CatalogRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItems_CatalogVersion_IsVerified_IsDeprecated",
                table: "ChecklistItems",
                columns: new[] { "CatalogVersion", "IsVerified", "IsDeprecated" });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItems_ProductId_CardNumber_Parallel_Variation",
                table: "ChecklistItems",
                columns: new[] { "ProductId", "CardNumber", "Parallel", "Variation" });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CatalogRecordId",
                table: "Cards",
                column: "CatalogRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CatalogVersion_IsVerified_IsDeprecated",
                table: "Cards",
                columns: new[] { "CatalogVersion", "IsVerified", "IsDeprecated" });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ProductId_ChecklistItemId_CatalogSourceRecordId",
                table: "Cards",
                columns: new[] { "ProductId", "ChecklistItemId", "CatalogSourceRecordId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_CatalogRecordId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CatalogSourceRecordId_CatalogVersion_IsVerified",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ChecklistItems_CatalogRecordId",
                table: "ChecklistItems");

            migrationBuilder.DropIndex(
                name: "IX_ChecklistItems_CatalogVersion_IsVerified_IsDeprecated",
                table: "ChecklistItems");

            migrationBuilder.DropIndex(
                name: "IX_ChecklistItems_ProductId_CardNumber_Parallel_Variation",
                table: "ChecklistItems");

            migrationBuilder.DropIndex(
                name: "IX_Cards_CatalogRecordId",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_CatalogVersion_IsVerified_IsDeprecated",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_ProductId_ChecklistItemId_CatalogSourceRecordId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "CatalogConfidence",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CatalogRecordId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CatalogSource",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CatalogSourceRecordId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CatalogUpdatedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CatalogVersion",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDeprecated",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsUserCreated",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ReplacedByCatalogRecordId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CatalogConfidence",
                table: "ChecklistItems");

            migrationBuilder.DropColumn(
                name: "CatalogRecordId",
                table: "ChecklistItems");

            migrationBuilder.DropColumn(
                name: "CatalogSource",
                table: "ChecklistItems");

            migrationBuilder.DropColumn(
                name: "CatalogSourceRecordId",
                table: "ChecklistItems");

            migrationBuilder.DropColumn(
                name: "CatalogUpdatedAt",
                table: "ChecklistItems");

            migrationBuilder.DropColumn(
                name: "CatalogVersion",
                table: "ChecklistItems");

            migrationBuilder.DropColumn(
                name: "IsDeprecated",
                table: "ChecklistItems");

            migrationBuilder.DropColumn(
                name: "IsUserCreated",
                table: "ChecklistItems");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "ChecklistItems");

            migrationBuilder.DropColumn(
                name: "ReplacedByCatalogRecordId",
                table: "ChecklistItems");

            migrationBuilder.DropColumn(
                name: "CatalogConfidence",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "CatalogRecordId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "CatalogSource",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "CatalogSourceRecordId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "CatalogUpdatedAt",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "CatalogVersion",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "IsDeprecated",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "IsUserCreated",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "ReplacedByCatalogRecordId",
                table: "Cards");
        }
    }
}

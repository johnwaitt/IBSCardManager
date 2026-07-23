using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IBSCardManager.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCatalogAndCardProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cards_CertNumber",
                table: "Cards");

            migrationBuilder.AlterColumn<string>(
                name: "Variety",
                table: "Cards",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "Cards",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Set",
                table: "Cards",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "GradeIssuer",
                table: "Cards",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Grade",
                table: "Cards",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(25)",
                oldMaxLength: 25,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CertNumber",
                table: "Cards",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CardNumber",
                table: "Cards",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AutographGrade",
                table: "Cards",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(25)",
                oldMaxLength: 25,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "Cards",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    BrandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrandName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.BrandId);
                });

            migrationBuilder.CreateTable(
                name: "Sports",
                columns: table => new
                {
                    SportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SportName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sports", x => x.SportId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "BrandId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Sports_SportId",
                        column: x => x.SportId,
                        principalTable: "Sports",
                        principalColumn: "SportId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Brands",
                columns: new[] { "BrandId", "BrandName", "IsActive" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Topps", true },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Bowman", true }
                });

            migrationBuilder.InsertData(
                table: "Sports",
                columns: new[] { "SportId", "IsActive", "SportName" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), true, "Baseball" });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CertNumber",
                table: "Cards",
                column: "CertNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ProductId",
                table: "Cards",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SportId",
                table: "Products",
                column: "SportId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Year_BrandId_ProductName",
                table: "Products",
                columns: new[] { "Year", "BrandId", "ProductName" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Products_ProductId",
                table: "Cards",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Products_ProductId",
                table: "Cards");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "Sports");

            migrationBuilder.DropIndex(
                name: "IX_Cards_CertNumber",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_ProductId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Cards");

            migrationBuilder.AlterColumn<string>(
                name: "Variety",
                table: "Cards",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "Cards",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Set",
                table: "Cards",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GradeIssuer",
                table: "Cards",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Grade",
                table: "Cards",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CertNumber",
                table: "Cards",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CardNumber",
                table: "Cards",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AutographGrade",
                table: "Cards",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CertNumber",
                table: "Cards",
                column: "CertNumber",
                unique: true,
                filter: "[CertNumber] IS NOT NULL");
        }
    }
}

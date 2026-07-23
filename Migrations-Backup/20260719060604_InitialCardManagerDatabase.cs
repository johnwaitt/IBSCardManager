using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IBSCardManager.Migrations
{
    /// <inheritdoc />
    public partial class InitialCardManagerDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "ChecklistItems",
                columns: table => new
                {
                    ChecklistItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Team = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Subset = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsRookie = table.Column<bool>(type: "bit", nullable: false),
                    IsAutograph = table.Column<bool>(type: "bit", nullable: false),
                    IsRelic = table.Column<bool>(type: "bit", nullable: false),
                    IsRefractor = table.Column<bool>(type: "bit", nullable: false),
                    StockImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistItems", x => x.ChecklistItemId);
                    table.ForeignKey(
                        name: "FK_ChecklistItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    CardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AutographGrade = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BackImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CardNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CertNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FrontImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StockImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ImageSourcePreference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    GradeIssuer = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsAutograph = table.Column<bool>(type: "bit", nullable: false),
                    IsRelic = table.Column<bool>(type: "bit", nullable: false),
                    IsRefractor = table.Column<bool>(type: "bit", nullable: false),
                    IsRookie = table.Column<bool>(type: "bit", nullable: false),
                    ListingPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ListingStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MyCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MyNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MyValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChecklistItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PsaEstimate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Serial = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Set = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    StorageBin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StorageBox = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StorageRow = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Team = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Variety = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.CardId);
                    table.ForeignKey(
                        name: "FK_Cards_ChecklistItems_ChecklistItemId",
                        column: x => x.ChecklistItemId,
                        principalTable: "ChecklistItems",
                        principalColumn: "ChecklistItemId");
                    table.ForeignKey(
                        name: "FK_Cards_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.SetNull);
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
                name: "IX_Cards_CardNumber",
                table: "Cards",
                column: "CardNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CertNumber",
                table: "Cards",
                column: "CertNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ChecklistItemId",
                table: "Cards",
                column: "ChecklistItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ProductId",
                table: "Cards",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_Set",
                table: "Cards",
                column: "Set");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_Subject",
                table: "Cards",
                column: "Subject");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_Team",
                table: "Cards",
                column: "Team");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItems_ProductId_CardNumber_Subject",
                table: "ChecklistItems",
                columns: new[] { "ProductId", "CardNumber", "Subject" });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "ChecklistItems");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "Sports");
        }
    }
}

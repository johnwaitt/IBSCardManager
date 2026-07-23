using System;
using IBSCardManager.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBSCardManager.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260723113000_AddCollectionAnalyticsTablesV170")]
    public class AddCollectionAnalyticsTablesV170 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CollectionSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalCards = table.Column<int>(type: "int", nullable: false),
                    UniqueCards = table.Column<int>(type: "int", nullable: false),
                    TotalQuantity = table.Column<int>(type: "int", nullable: false),
                    TotalEstimatedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCostBasis = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnrealizedGain = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnrealizedLoss = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RealizedProfit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ListedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoldValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GradedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RawValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HighConfidenceValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LowConfidenceValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ApplicationVersion = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SchemaVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryAnalyticsSummaries",
                columns: table => new
                {
                    InventoryCardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentEstimatedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostBasis = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnrealizedGainLoss = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ROI = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    PriceConfidence = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PriceFreshness = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    GradingScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SellScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    HoldScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    LiquidityScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DemandScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Recommendation = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RecommendationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LastCalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAnalyticsSummaries", x => x.InventoryCardId);
                    table.ForeignKey(
                        name: "FK_InventoryAnalyticsSummaries_Cards_InventoryCardId",
                        column: x => x.InventoryCardId,
                        principalTable: "Cards",
                        principalColumn: "CardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryCardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecommendationType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Confidence = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Reasons = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Risks = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredActions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Accepted = table.Column<bool>(type: "bit", nullable: false),
                    Dismissed = table.Column<bool>(type: "bit", nullable: false),
                    SnoozedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModelVersion = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RuleVersion = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommendationRecords_Cards_InventoryCardId",
                        column: x => x.InventoryCardId,
                        principalTable: "Cards",
                        principalColumn: "CardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSnapshots_SnapshotDate",
                table: "CollectionSnapshots",
                column: "SnapshotDate");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAnalyticsSummaries_LastCalculatedAt",
                table: "InventoryAnalyticsSummaries",
                column: "LastCalculatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationRecords_InventoryCardId_RecommendationType_GeneratedAt",
                table: "RecommendationRecords",
                columns: new[] { "InventoryCardId", "RecommendationType", "GeneratedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionSnapshots");

            migrationBuilder.DropTable(
                name: "InventoryAnalyticsSummaries");

            migrationBuilder.DropTable(
                name: "RecommendationRecords");
        }
    }
}

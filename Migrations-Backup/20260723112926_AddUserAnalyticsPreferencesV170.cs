using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBSCardManager.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAnalyticsPreferencesV170 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAnalyticsPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NotifyPriceIncrease = table.Column<bool>(type: "bit", nullable: false),
                    NotifyPriceDecrease = table.Column<bool>(type: "bit", nullable: false),
                    NotifyGradingOpportunity = table.Column<bool>(type: "bit", nullable: false),
                    NotifyStaleValuation = table.Column<bool>(type: "bit", nullable: false),
                    NotifyReadyToList = table.Column<bool>(type: "bit", nullable: false),
                    NotifyListingSold = table.Column<bool>(type: "bit", nullable: false),
                    NotifyDuplicateInventory = table.Column<bool>(type: "bit", nullable: false),
                    NotifyMissingCostBasis = table.Column<bool>(type: "bit", nullable: false),
                    NotifyLowConfidenceIdentification = table.Column<bool>(type: "bit", nullable: false),
                    CollectionGoal = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MinimumCashFlowTarget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAnalyticsPreferences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAnalyticsPreferences_ProfileName",
                table: "UserAnalyticsPreferences",
                column: "ProfileName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAnalyticsPreferences");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBSCardManager.Migrations
{
    /// <inheritdoc />
    public partial class InitialCardDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    CardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Team = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true),
                    Set = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Variety = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Serial = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GradeIssuer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Grade = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    AutographGrade = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    CertNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsRookie = table.Column<bool>(type: "bit", nullable: false),
                    IsAutograph = table.Column<bool>(type: "bit", nullable: false),
                    IsRelic = table.Column<bool>(type: "bit", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    MyCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PsaEstimate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MyValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ListingPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ListingStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StorageBox = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StorageRow = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StorageBin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FrontImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BackImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MyNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.CardId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CardNumber",
                table: "Cards",
                column: "CardNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CertNumber",
                table: "Cards",
                column: "CertNumber",
                unique: true,
                filter: "[CertNumber] IS NOT NULL");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cards");
        }
    }
}

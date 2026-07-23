using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBSCardManager.Migrations
{
    /// <inheritdoc />
    public partial class AddKnowledgeEngineCoreV220 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DecisionHistoryRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectType = table.Column<int>(type: "int", nullable: false),
                    SubjectStableId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DecisionType = table.Column<int>(type: "int", nullable: false),
                    DecisionStatus = table.Column<int>(type: "int", nullable: false),
                    SelectedOption = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AlternativeOptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExplanationSummary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EvidenceCount = table.Column<int>(type: "int", nullable: false),
                    MissingDataSummary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UserAction = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PreviousDecisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModelVersion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PromptVersion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RuleVersion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ApplicationVersion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CatalogVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionHistoryRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeAuditRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperationType = table.Column<int>(type: "int", nullable: false),
                    SubjectStableId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApplicationVersion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CatalogVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ModelVersion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RuleVersion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    BeforeValuesJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AfterValuesJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OperationResult = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeAuditRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StableId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    KnowledgeType = table.Column<int>(type: "int", nullable: false),
                    SubjectType = table.Column<int>(type: "int", nullable: false),
                    SubjectStableId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    StatementKey = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    StatementValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NormalizedValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VerificationLevel = table.Column<int>(type: "int", nullable: false),
                    SourceCount = table.Column<int>(type: "int", nullable: false),
                    UserConfirmationCount = table.Column<int>(type: "int", nullable: false),
                    UserCorrectionCount = table.Column<int>(type: "int", nullable: false),
                    MarketplaceConfirmationCount = table.Column<int>(type: "int", nullable: false),
                    CatalogConfirmationCount = table.Column<int>(type: "int", nullable: false),
                    ImageMatchConfirmationCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeprecated = table.Column<bool>(type: "bit", nullable: false),
                    ReplacedByKnowledgeRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FirstObservedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastObservedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RuleVersion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeReviewItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubjectType = table.Column<int>(type: "int", nullable: false),
                    SubjectStableId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    KnowledgeRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RuleVersion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeReviewItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCorrections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectType = table.Column<int>(type: "int", nullable: false),
                    SubjectStableId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OriginalValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CorrectedValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CorrectionType = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AppliedToCurrentRecord = table.Column<bool>(type: "bit", nullable: false),
                    EligibleForLearning = table.Column<bool>(type: "bit", nullable: false),
                    LearningStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModelVersion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RuleVersion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCorrections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeEvidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KnowledgeRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvidenceType = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    SourceRecordId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SourceUri = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EvidenceSummary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RawValue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NormalizedValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConfidenceContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsSupporting = table.Column<bool>(type: "bit", nullable: false),
                    IsContradicting = table.Column<bool>(type: "bit", nullable: false),
                    ObservedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RuleVersion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnowledgeEvidence_KnowledgeRecords_KnowledgeRecordId",
                        column: x => x.KnowledgeRecordId,
                        principalTable: "KnowledgeRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DecisionHistoryRecords_SubjectType_SubjectStableId_CreatedAt",
                table: "DecisionHistoryRecords",
                columns: new[] { "SubjectType", "SubjectStableId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeAuditRecords_CreatedAt",
                table: "KnowledgeAuditRecords",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeEvidence_KnowledgeRecordId",
                table: "KnowledgeEvidence",
                column: "KnowledgeRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeRecords_ConfidenceScore",
                table: "KnowledgeRecords",
                column: "ConfidenceScore");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeRecords_CreatedAt",
                table: "KnowledgeRecords",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeRecords_KnowledgeType",
                table: "KnowledgeRecords",
                column: "KnowledgeType");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeRecords_StableId",
                table: "KnowledgeRecords",
                column: "StableId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeRecords_SubjectType_SubjectStableId",
                table: "KnowledgeRecords",
                columns: new[] { "SubjectType", "SubjectStableId" });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeRecords_UpdatedAt",
                table: "KnowledgeRecords",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeRecords_VerificationLevel",
                table: "KnowledgeRecords",
                column: "VerificationLevel");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeReviewItems_CreatedAt",
                table: "KnowledgeReviewItems",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeReviewItems_Status",
                table: "KnowledgeReviewItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserCorrections_LearningStatus",
                table: "UserCorrections",
                column: "LearningStatus");

            migrationBuilder.CreateIndex(
                name: "IX_UserCorrections_SubjectType_SubjectStableId",
                table: "UserCorrections",
                columns: new[] { "SubjectType", "SubjectStableId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DecisionHistoryRecords");

            migrationBuilder.DropTable(
                name: "KnowledgeAuditRecords");

            migrationBuilder.DropTable(
                name: "KnowledgeEvidence");

            migrationBuilder.DropTable(
                name: "KnowledgeReviewItems");

            migrationBuilder.DropTable(
                name: "UserCorrections");

            migrationBuilder.DropTable(
                name: "KnowledgeRecords");
        }
    }
}

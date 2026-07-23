using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IBSCardManager.Migrations
{
    /// <inheritdoc />
    public partial class InitialEnterpriseDatabase : Migration
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
                    BrandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChecklistAvailabilityStatus = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LastChecklistImportSource = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChecklistLastImportedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CatalogRecordId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CatalogSource = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CatalogSourceRecordId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CatalogVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CatalogUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CatalogConfidence = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsUserCreated = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsDeprecated = table.Column<bool>(type: "bit", nullable: false),
                    ReplacedByCatalogRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                name: "ChecklistImportHistories",
                columns: table => new
                {
                    ChecklistImportHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SourceUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SourceFile = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceProductIdentifier = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SourceVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    LicenseUsageNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImportProfile = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    VerificationStatus = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    RetrievedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ImportedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowsRead = table.Column<int>(type: "int", nullable: false),
                    RowsImported = table.Column<int>(type: "int", nullable: false),
                    RowsUpdated = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistImportHistories", x => x.ChecklistImportHistoryId);
                    table.ForeignKey(
                        name: "FK_ChecklistImportHistories_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistItems",
                columns: table => new
                {
                    ChecklistItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AdditionalSubjects = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Team = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AdditionalTeams = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Subset = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsRookie = table.Column<bool>(type: "bit", nullable: false),
                    IsAutograph = table.Column<bool>(type: "bit", nullable: false),
                    IsRelic = table.Column<bool>(type: "bit", nullable: false),
                    IsRefractor = table.Column<bool>(type: "bit", nullable: false),
                    StockImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Parallel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Variation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PrintRun = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StockBackImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SourceName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    SourceType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SourceFile = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceProductIdentifier = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SourceCardIdentifier = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SourceDateRetrievedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceDateImportedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    SourceLicenseUsageNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImportProfile = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    SourceOriginalRowNumber = table.Column<int>(type: "int", nullable: true),
                    SourceRawValuesJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SourceVerificationStatus = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ReferenceImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReferencePageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReferenceImageSource = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ReferenceImageDateLocatedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenceImageUsageStatus = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CachedThumbnailPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReferenceImageHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReferenceImageVerificationStatus = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CatalogRecordId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CatalogSource = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CatalogSourceRecordId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CatalogVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CatalogUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CatalogConfidence = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsUserCreated = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsDeprecated = table.Column<bool>(type: "bit", nullable: false),
                    ReplacedByCatalogRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                name: "WebSearchResults",
                columns: table => new
                {
                    WebSearchResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SearchScope = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SearchQuery = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PageSource = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DateRetrievedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebSearchResults", x => x.WebSearchResultId);
                    table.ForeignKey(
                        name: "FK_WebSearchResults_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.SetNull);
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
                    Year = table.Column<int>(type: "int", nullable: true),
                    EbaySku = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    EbayCategoryId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    EbayCondition = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    EbayTitle = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    EbayDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ListingFormat = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BestOfferEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ShippingPolicyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ReturnPolicyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PaymentPolicyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PackageWeightOz = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    PackageLengthIn = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    PackageWidthIn = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    PackageHeightIn = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CatalogRecordId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CatalogSource = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CatalogSourceRecordId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CatalogVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CatalogUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CatalogConfidence = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsUserCreated = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsDeprecated = table.Column<bool>(type: "bit", nullable: false),
                    ReplacedByCatalogRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                name: "IX_Cards_CatalogRecordId",
                table: "Cards",
                column: "CatalogRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CatalogVersion_IsVerified_IsDeprecated",
                table: "Cards",
                columns: new[] { "CatalogVersion", "IsVerified", "IsDeprecated" });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CertNumber",
                table: "Cards",
                column: "CertNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ChecklistItemId",
                table: "Cards",
                column: "ChecklistItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ProductId_ChecklistItemId_CatalogSourceRecordId",
                table: "Cards",
                columns: new[] { "ProductId", "ChecklistItemId", "CatalogSourceRecordId" });

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
                name: "IX_ChecklistImportHistories_ProductId_ImportedUtc",
                table: "ChecklistImportHistories",
                columns: new[] { "ProductId", "ImportedUtc" });

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
                name: "IX_ChecklistItems_ProductId_CardNumber_Subject",
                table: "ChecklistItems",
                columns: new[] { "ProductId", "CardNumber", "Subject" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSnapshots_SnapshotDate",
                table: "CollectionSnapshots",
                column: "SnapshotDate");

            migrationBuilder.CreateIndex(
                name: "IX_DecisionHistoryRecords_SubjectType_SubjectStableId_CreatedAt",
                table: "DecisionHistoryRecords",
                columns: new[] { "SubjectType", "SubjectStableId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAnalyticsSummaries_LastCalculatedAt",
                table: "InventoryAnalyticsSummaries",
                column: "LastCalculatedAt");

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
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CatalogRecordId",
                table: "Products",
                column: "CatalogRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CatalogSourceRecordId_CatalogVersion_IsVerified",
                table: "Products",
                columns: new[] { "CatalogSourceRecordId", "CatalogVersion", "IsVerified" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_SportId",
                table: "Products",
                column: "SportId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Year_BrandId_ProductName",
                table: "Products",
                columns: new[] { "Year", "BrandId", "ProductName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationRecords_InventoryCardId_RecommendationType_GeneratedAt",
                table: "RecommendationRecords",
                columns: new[] { "InventoryCardId", "RecommendationType", "GeneratedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAnalyticsPreferences_ProfileName",
                table: "UserAnalyticsPreferences",
                column: "ProfileName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCorrections_LearningStatus",
                table: "UserCorrections",
                column: "LearningStatus");

            migrationBuilder.CreateIndex(
                name: "IX_UserCorrections_SubjectType_SubjectStableId",
                table: "UserCorrections",
                columns: new[] { "SubjectType", "SubjectStableId" });

            migrationBuilder.CreateIndex(
                name: "IX_WebSearchResults_ProductId_SearchScope_DateRetrievedUtc",
                table: "WebSearchResults",
                columns: new[] { "ProductId", "SearchScope", "DateRetrievedUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChecklistImportHistories");

            migrationBuilder.DropTable(
                name: "CollectionSnapshots");

            migrationBuilder.DropTable(
                name: "DecisionHistoryRecords");

            migrationBuilder.DropTable(
                name: "InventoryAnalyticsSummaries");

            migrationBuilder.DropTable(
                name: "KnowledgeAuditRecords");

            migrationBuilder.DropTable(
                name: "KnowledgeEvidence");

            migrationBuilder.DropTable(
                name: "KnowledgeReviewItems");

            migrationBuilder.DropTable(
                name: "RecommendationRecords");

            migrationBuilder.DropTable(
                name: "UserAnalyticsPreferences");

            migrationBuilder.DropTable(
                name: "UserCorrections");

            migrationBuilder.DropTable(
                name: "WebSearchResults");

            migrationBuilder.DropTable(
                name: "KnowledgeRecords");

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

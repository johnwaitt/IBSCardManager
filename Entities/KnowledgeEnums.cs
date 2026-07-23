namespace IBSCardManager.Entities;

public enum KnowledgeType
{
    CardIdentity = 1,
    ProductMatch = 2,
    ChecklistMatch = 3,
    PlayerNameAlias = 4,
    TeamNameAlias = 5,
    ParallelIdentification = 6,
    VariationIdentification = 7,
    OcrCorrection = 8,
    ImageRecognitionPattern = 9,
    PricingPattern = 10,
    UserPreference = 11,
    MarketplaceObservation = 12
}

public enum KnowledgeSubjectType
{
    Unknown = 0,
    ScannerPair = 1,
    InventoryCard = 2,
    ChecklistCard = 3,
    Product = 4,
    Player = 5,
    Team = 6,
    PricingResearch = 7,
    MarketplaceListing = 8
}

public enum KnowledgeVerificationLevel
{
    Unverified = 0,
    UserConfirmed = 1,
    CatalogSupported = 2,
    MultiSourceSupported = 3,
    Verified = 4,
    Disputed = 5,
    Deprecated = 6
}

public enum KnowledgeEvidenceType
{
    MasterCatalog = 1,
    UserConfirmation = 2,
    UserCorrection = 3,
    Ocr = 4,
    ImageAnalysis = 5,
    MarketplaceComparable = 6,
    SoldListing = 7,
    ActiveListing = 8,
    ChecklistImport = 9,
    ReferenceImage = 10,
    GradingResult = 11,
    ManualResearch = 12
}

public enum KnowledgeEvidenceSourceType
{
    Unknown = 0,
    Scanner = 1,
    Catalog = 2,
    User = 3,
    Marketplace = 4,
    Pricing = 5,
    Import = 6,
    System = 7
}

public enum CorrectionType
{
    Identity = 1,
    Product = 2,
    Set = 3,
    CardNumber = 4,
    Player = 5,
    Team = 6,
    Parallel = 7,
    Variation = 8,
    Year = 9,
    Manufacturer = 10,
    Ocr = 11,
    ImageOrientation = 12,
    Pricing = 13,
    Other = 14
}

public enum LearningStatus
{
    PendingReview = 1,
    Approved = 2,
    Rejected = 3,
    Applied = 4,
    Superseded = 5
}

public enum DecisionType
{
    CardIdentification = 1,
    CandidateSelection = 2,
    CatalogReconciliation = 3,
    PricingValuation = 4,
    GradingRecommendation = 5,
    SellRecommendation = 6,
    HoldRecommendation = 7,
    DuplicateResolution = 8,
    MarketplaceListingDecision = 9
}

public enum DecisionStatus
{
    Proposed = 1,
    Confirmed = 2,
    Corrected = 3,
    Rejected = 4,
    Superseded = 5,
    Failed = 6
}

public enum ConfidenceClassification
{
    InsufficientEvidence = 0,
    VeryLow = 1,
    Low = 2,
    Medium = 3,
    High = 4,
    VeryHigh = 5,
    Verified = 6
}

public enum KnowledgeHealthState
{
    Healthy = 1,
    Warning = 2,
    Failed = 3,
    NotConfigured = 4
}

public enum KnowledgeReviewItemType
{
    ProposedAlias = 1,
    RepeatedOcrCorrection = 2,
    ConflictingIdentification = 3,
    LowConfidenceKnowledgeRecord = 4,
    DisputedRecord = 5,
    ProposedCatalogFix = 6,
    ProposedScannerRankingAdjustment = 7,
    RepeatedUserCorrection = 8,
    MissingEvidence = 9,
    StaleKnowledge = 10,
    HighImpactLearningAction = 11
}

public enum KnowledgeReviewQueueState
{
    New = 1,
    InReview = 2,
    Approved = 3,
    Rejected = 4,
    Deferred = 5,
    Applied = 6,
    Superseded = 7
}

public enum KnowledgeAuditOperationType
{
    KnowledgeRecordCreated = 1,
    EvidenceAdded = 2,
    EvidenceContradicted = 3,
    CorrectionSubmitted = 4,
    CorrectionApproved = 5,
    ConfidenceRecalculated = 6,
    VerificationLevelChanged = 7,
    ReviewItemApproved = 8,
    ReviewItemRejected = 9,
    DecisionConfirmed = 10,
    DecisionCorrected = 11,
    RecordDeprecated = 12
}

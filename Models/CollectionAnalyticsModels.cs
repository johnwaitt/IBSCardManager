namespace IBSCardManager.Models;

public sealed class CollectionAnalyticsDashboardViewModel
{
    public string Range { get; init; } = "all";
    public decimal TotalCollectionValue { get; init; }
    public decimal TotalCostBasis { get; init; }
    public decimal UnrealizedGainLoss { get; init; }
    public decimal UnrealizedGain { get; init; }
    public decimal UnrealizedLoss { get; init; }
    public decimal RealizedProfit { get; init; }
    public int TotalCards { get; init; }
    public int UniqueCards { get; init; }
    public int RawCards { get; init; }
    public int GradedCards { get; init; }
    public decimal RawValue { get; init; }
    public decimal GradedValue { get; init; }
    public decimal ListedValue { get; init; }
    public decimal UnlistedValue { get; init; }
    public decimal HighConfidenceValue { get; init; }
    public decimal LowConfidenceValue { get; init; }
    public int CardsNeedingPricing { get; init; }
    public int CardsWithStalePricing { get; init; }
    public int CardsReadyToList { get; init; }
    public int StrongGradingCandidates { get; init; }
    public int RecentSales { get; init; }
    public int RecentAcquisitions { get; init; }
    public DateTimeOffset LastCalculatedAt { get; init; }
    public IReadOnlyList<CollectionSnapshotPointViewModel> History { get; init; } = Array.Empty<CollectionSnapshotPointViewModel>();
}

public sealed class CollectionSnapshotPointViewModel
{
    public DateTime SnapshotDate { get; init; }
    public decimal TotalEstimatedValue { get; init; }
    public decimal TotalCostBasis { get; init; }
    public decimal UnrealizedGainLoss { get; init; }
    public decimal RealizedProfit { get; init; }
    public decimal RawValue { get; init; }
    public decimal GradedValue { get; init; }
}

public sealed class AnalyticsCardScoreViewModel
{
    public Guid CardId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? SetName { get; init; }
    public string? CardNumber { get; init; }
    public string? ImageUrl { get; init; }
    public decimal CurrentEstimatedValue { get; init; }
    public decimal CostBasis { get; init; }
    public decimal UnrealizedGainLoss { get; init; }
    public decimal RoiPercent { get; init; }
    public decimal Score { get; init; }
    public decimal Confidence { get; init; }
    public string Recommendation { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string Risks { get; init; } = string.Empty;
    public decimal ExpectedGradedValue { get; init; }
    public decimal ExpectedNetProfit { get; init; }
    public decimal ExpectedRoiPercent { get; init; }
    public string BreakEvenGrade { get; init; } = "N/A";
    public decimal MinTopGradeProbability { get; init; }
    public string BestGrader { get; init; } = "N/A";
}

public sealed class CollectionConcentrationRowViewModel
{
    public string Dimension { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public int CardCount { get; init; }
    public decimal TotalValue { get; init; }
    public decimal CostBasis { get; init; }
    public decimal UnrealizedGainLoss { get; init; }
    public decimal PercentageOfCollectionValue { get; init; }
}

public sealed class DuplicateAnalyticsRowViewModel
{
    public string GroupKey { get; init; } = string.Empty;
    public int Copies { get; init; }
    public int RawCopies { get; init; }
    public int GradedCopies { get; init; }
    public string Recommendation { get; init; } = string.Empty;
}

public sealed class DataQualityIssueViewModel
{
    public Guid CardId { get; init; }
    public string CardIdentity { get; init; } = string.Empty;
    public decimal Score { get; init; }
    public string Classification { get; init; } = "Needs review";
    public IReadOnlyList<string> MissingFields { get; init; } = Array.Empty<string>();
}

public sealed class RecommendationCenterViewModel
{
    public IReadOnlyList<AnalyticsCardScoreViewModel> GradeThese { get; init; } = Array.Empty<AnalyticsCardScoreViewModel>();
    public IReadOnlyList<AnalyticsCardScoreViewModel> SellThese { get; init; } = Array.Empty<AnalyticsCardScoreViewModel>();
    public IReadOnlyList<AnalyticsCardScoreViewModel> HoldThese { get; init; } = Array.Empty<AnalyticsCardScoreViewModel>();
    public IReadOnlyList<DataQualityIssueViewModel> FixTheseRecords { get; init; } = Array.Empty<DataQualityIssueViewModel>();
    public IReadOnlyList<DuplicateAnalyticsRowViewModel> ListTheseDuplicates { get; init; } = Array.Empty<DuplicateAnalyticsRowViewModel>();
    public DateTimeOffset LastCalculatedAt { get; init; }
}

public sealed class SnapshotCreationResult
{
    public bool Created { get; init; }
    public string Reason { get; init; } = string.Empty;
}

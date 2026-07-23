using IBSCardManager.Entities;

namespace IBSCardManager.Models;

public sealed class ConfidenceFactor
{
    public string Name { get; init; } = string.Empty;
    public decimal Impact { get; init; }
    public string Detail { get; init; } = string.Empty;
}

public sealed class ConfidenceScoreResult
{
    public decimal Score { get; init; }
    public ConfidenceClassification Classification { get; init; }
    public IReadOnlyList<ConfidenceFactor> SupportingFactors { get; init; } = Array.Empty<ConfidenceFactor>();
    public IReadOnlyList<ConfidenceFactor> ContradictingFactors { get; init; } = Array.Empty<ConfidenceFactor>();
    public IReadOnlyList<ConfidenceFactor> MissingDataFactors { get; init; } = Array.Empty<ConfidenceFactor>();
    public string RuleVersion { get; init; } = string.Empty;
    public DateTimeOffset CalculatedAt { get; init; }
}

public sealed class ConfidenceScoringInput
{
    public bool MasterCatalogExactMatch { get; init; }
    public bool StableIdMatch { get; init; }
    public bool ExternalSourceMatch { get; init; }
    public bool CardNumberMatch { get; init; }
    public bool PlayerMatch { get; init; }
    public bool TeamMatch { get; init; }
    public bool ProductMatch { get; init; }
    public bool YearMatch { get; init; }
    public bool ParallelMatch { get; init; }
    public bool VariationMatch { get; init; }
    public decimal OcrQuality { get; init; }
    public decimal ImageQuality { get; init; }
    public bool ReferenceImageMatch { get; init; }
    public int UserConfirmations { get; init; }
    public int UserCorrections { get; init; }
    public int MarketplaceSupportCount { get; init; }
    public int ContradictingEvidenceCount { get; init; }
    public int MissingRequiredFieldCount { get; init; }
    public int SourceFreshnessDays { get; init; }
}

public sealed class KnowledgeVersionInfo
{
    public string? AiModelName { get; init; }
    public string? AiModelVersion { get; init; }
    public string? PromptTemplateVersion { get; init; }
    public string ConfidenceRuleVersion { get; init; } = "knowledge-confidence-rules-v1";
    public string KnowledgeSchemaVersion { get; init; } = "knowledge-schema-v1";
    public string LearningRuleVersion { get; init; } = "knowledge-learning-rules-v1";
}

public sealed class KnowledgeHealthIssue
{
    public string CheckCode { get; init; } = string.Empty;
    public KnowledgeHealthState State { get; init; }
    public string Summary { get; init; } = string.Empty;
}

public sealed class KnowledgeHealthReport
{
    public KnowledgeHealthState OverallState { get; init; }
    public IReadOnlyList<KnowledgeHealthIssue> Issues { get; init; } = Array.Empty<KnowledgeHealthIssue>();
}

public sealed class DecisionExplanationInput
{
    public string? SelectedOption { get; init; }
    public decimal ConfidenceScore { get; init; }
    public IReadOnlyList<string> StrongestSupportingFactors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ImportantContradictions { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingInformation { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> AlternativesConsidered { get; init; } = Array.Empty<string>();
    public string? UserAction { get; init; }
}

public sealed class KnowledgeReviewQueueSummary
{
    public int PendingCount { get; init; }
    public int DisputedCount { get; init; }
    public int LowConfidenceCount { get; init; }
}

public sealed class KnowledgeLearningDecision
{
    public bool ApplyAutomatically { get; init; }
    public bool QueueForReview { get; init; }
    public string Reason { get; init; } = string.Empty;
    public KnowledgeReviewItemType? ReviewItemType { get; init; }
}

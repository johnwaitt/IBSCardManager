using IBSCardManager.Entities;

namespace IBSCardManager.Models;

public sealed class KnowledgeReviewItemViewModel
{
    public Guid Id { get; init; }
    public KnowledgeReviewItemType ItemType { get; init; }
    public KnowledgeReviewQueueState Status { get; init; }
    public KnowledgeSubjectType SubjectType { get; init; }
    public string SubjectStableId { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReviewedAt { get; init; }
}

public sealed class KnowledgeReviewQueueViewModel
{
    public IReadOnlyList<KnowledgeReviewItemViewModel> Items { get; init; } = Array.Empty<KnowledgeReviewItemViewModel>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}

public sealed class DecisionHistoryRowViewModel
{
    public Guid Id { get; init; }
    public DecisionType DecisionType { get; init; }
    public DecisionStatus DecisionStatus { get; init; }
    public string? SelectedOption { get; init; }
    public decimal ConfidenceScore { get; init; }
    public string? ExplanationSummary { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public sealed class KnowledgeRecordDetailViewModel
{
    public KnowledgeRecord Record { get; init; } = new();
    public IReadOnlyList<KnowledgeEvidence> SupportingEvidence { get; init; } = Array.Empty<KnowledgeEvidence>();
    public IReadOnlyList<KnowledgeEvidence> ContradictingEvidence { get; init; } = Array.Empty<KnowledgeEvidence>();
    public IReadOnlyList<DecisionHistoryRowViewModel> DecisionHistory { get; init; } = Array.Empty<DecisionHistoryRowViewModel>();
    public IReadOnlyList<Guid> RelatedInventoryRecords { get; init; } = Array.Empty<Guid>();
    public IReadOnlyList<string> RelatedScannerSessions { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RelatedPricingResearch { get; init; } = Array.Empty<string>();
    public IReadOnlyList<KnowledgeAuditRecord> AuditHistory { get; init; } = Array.Empty<KnowledgeAuditRecord>();
}

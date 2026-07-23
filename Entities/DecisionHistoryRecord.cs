using System.ComponentModel.DataAnnotations;

namespace IBSCardManager.Entities;

public sealed class DecisionHistoryRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public KnowledgeSubjectType SubjectType { get; set; }

    [StringLength(150)]
    public string SubjectStableId { get; set; } = string.Empty;

    public DecisionType DecisionType { get; set; }
    public DecisionStatus DecisionStatus { get; set; }

    [StringLength(300)]
    public string? SelectedOption { get; set; }

    public string? AlternativeOptionsJson { get; set; }
    public decimal ConfidenceScore { get; set; }

    [StringLength(1000)]
    public string? ExplanationSummary { get; set; }

    public int EvidenceCount { get; set; }

    [StringLength(1000)]
    public string? MissingDataSummary { get; set; }

    [StringLength(200)]
    public string? UserAction { get; set; }

    public Guid? PreviousDecisionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    [StringLength(120)]
    public string? ModelVersion { get; set; }

    [StringLength(120)]
    public string? PromptVersion { get; set; }

    [StringLength(120)]
    public string? RuleVersion { get; set; }

    [StringLength(40)]
    public string? ApplicationVersion { get; set; }

    [StringLength(80)]
    public string? CatalogVersion { get; set; }
}

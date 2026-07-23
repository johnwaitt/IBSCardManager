using System.ComponentModel.DataAnnotations;

namespace IBSCardManager.Entities;

public sealed class KnowledgeRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [StringLength(150)]
    public string StableId { get; set; } = string.Empty;

    public KnowledgeType KnowledgeType { get; set; }
    public KnowledgeSubjectType SubjectType { get; set; }

    [StringLength(150)]
    public string SubjectStableId { get; set; } = string.Empty;

    [StringLength(120)]
    public string StatementKey { get; set; } = string.Empty;

    [StringLength(500)]
    public string StatementValue { get; set; } = string.Empty;

    [StringLength(500)]
    public string? NormalizedValue { get; set; }

    public decimal ConfidenceScore { get; set; }
    public KnowledgeVerificationLevel VerificationLevel { get; set; } = KnowledgeVerificationLevel.Unverified;
    public int SourceCount { get; set; }
    public int UserConfirmationCount { get; set; }
    public int UserCorrectionCount { get; set; }
    public int MarketplaceConfirmationCount { get; set; }
    public int CatalogConfirmationCount { get; set; }
    public int ImageMatchConfirmationCount { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeprecated { get; set; }
    public Guid? ReplacedByKnowledgeRecordId { get; set; }
    public DateTime FirstObservedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastObservedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastVerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(120)]
    public string? ModelVersion { get; set; }

    [StringLength(120)]
    public string? RuleVersion { get; set; }

    public List<KnowledgeEvidence> Evidence { get; set; } = new();
}

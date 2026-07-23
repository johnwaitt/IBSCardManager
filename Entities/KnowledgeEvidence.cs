using System.ComponentModel.DataAnnotations;

namespace IBSCardManager.Entities;

public sealed class KnowledgeEvidence
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid KnowledgeRecordId { get; set; }
    public KnowledgeRecord? KnowledgeRecord { get; set; }
    public KnowledgeEvidenceType EvidenceType { get; set; }
    public KnowledgeEvidenceSourceType SourceType { get; set; }

    [StringLength(150)]
    public string? SourceRecordId { get; set; }

    [StringLength(1000)]
    public string? SourceUri { get; set; }

    [StringLength(500)]
    public string? EvidenceSummary { get; set; }

    [StringLength(2000)]
    public string? RawValue { get; set; }

    [StringLength(500)]
    public string? NormalizedValue { get; set; }

    public decimal ConfidenceContribution { get; set; }
    public bool IsSupporting { get; set; }
    public bool IsContradicting { get; set; }
    public DateTime ObservedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(120)]
    public string? ModelVersion { get; set; }

    [StringLength(120)]
    public string? RuleVersion { get; set; }
}

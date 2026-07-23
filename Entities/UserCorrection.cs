using System.ComponentModel.DataAnnotations;

namespace IBSCardManager.Entities;

public sealed class UserCorrection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public KnowledgeSubjectType SubjectType { get; set; }

    [StringLength(150)]
    public string SubjectStableId { get; set; } = string.Empty;

    [StringLength(500)]
    public string? OriginalValue { get; set; }

    [StringLength(500)]
    public string CorrectedValue { get; set; } = string.Empty;

    [StringLength(120)]
    public string FieldName { get; set; } = string.Empty;

    public CorrectionType CorrectionType { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    [StringLength(2000)]
    public string? UserNotes { get; set; }

    public bool AppliedToCurrentRecord { get; set; }
    public bool EligibleForLearning { get; set; }
    public LearningStatus LearningStatus { get; set; } = LearningStatus.PendingReview;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    [StringLength(120)]
    public string? ModelVersion { get; set; }

    [StringLength(120)]
    public string? RuleVersion { get; set; }
}

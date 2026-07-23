using System.ComponentModel.DataAnnotations;

namespace IBSCardManager.Entities;

public sealed class KnowledgeReviewItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public KnowledgeReviewItemType ItemType { get; set; }
    public KnowledgeReviewQueueState Status { get; set; } = KnowledgeReviewQueueState.New;
    public KnowledgeSubjectType SubjectType { get; set; }

    [StringLength(150)]
    public string SubjectStableId { get; set; } = string.Empty;

    public Guid? KnowledgeRecordId { get; set; }

    [StringLength(300)]
    public string Summary { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Notes { get; set; }

    public int Priority { get; set; } = 50;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    [StringLength(120)]
    public string? RuleVersion { get; set; }
}

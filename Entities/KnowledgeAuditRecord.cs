using System.ComponentModel.DataAnnotations;

namespace IBSCardManager.Entities;

public sealed class KnowledgeAuditRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public KnowledgeAuditOperationType OperationType { get; set; }

    [StringLength(150)]
    public string? SubjectStableId { get; set; }

    [StringLength(100)]
    public string UserId { get; set; } = "system";

    [StringLength(40)]
    public string? ApplicationVersion { get; set; }

    [StringLength(80)]
    public string? CatalogVersion { get; set; }

    [StringLength(120)]
    public string? ModelVersion { get; set; }

    [StringLength(120)]
    public string? RuleVersion { get; set; }

    [StringLength(2000)]
    public string? BeforeValuesJson { get; set; }

    [StringLength(2000)]
    public string? AfterValuesJson { get; set; }

    [StringLength(200)]
    public string OperationResult { get; set; } = "Success";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

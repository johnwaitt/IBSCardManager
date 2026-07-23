using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBSCardManager.Entities;

public sealed class RecommendationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InventoryCardId { get; set; }
    public Card? InventoryCard { get; set; }

    [Required]
    [StringLength(64)]
    public string RecommendationType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(5,2)")]
    public decimal Score { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal Confidence { get; set; }

    [Required]
    [StringLength(400)]
    public string Summary { get; set; } = string.Empty;

    [Required]
    public string Reasons { get; set; } = string.Empty;

    [Required]
    public string Risks { get; set; } = string.Empty;

    [Required]
    public string RequiredActions { get; set; } = string.Empty;

    public DateTime GeneratedAt { get; set; }
    public bool Accepted { get; set; }
    public bool Dismissed { get; set; }
    public DateTime? SnoozedUntil { get; set; }
    public string? UserNotes { get; set; }

    [Required]
    [StringLength(32)]
    public string ModelVersion { get; set; } = "2.2.0";

    [Required]
    [StringLength(32)]
    public string RuleVersion { get; set; } = "2.2.0";
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBSCardManager.Entities;

public sealed class CollectionSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime SnapshotDate { get; set; }
    public int TotalCards { get; set; }
    public int UniqueCards { get; set; }
    public int TotalQuantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalEstimatedValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCostBasis { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnrealizedGain { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnrealizedLoss { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal RealizedProfit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ListedValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SoldValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal GradedValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal RawValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal HighConfidenceValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LowConfidenceValue { get; set; }

    [Required]
    [StringLength(32)]
    public string ApplicationVersion { get; set; } = "2.2.0";

    [Required]
    [StringLength(64)]
    public string SchemaVersion { get; set; } = string.Empty;
}

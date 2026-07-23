using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBSCardManager.Entities;

public sealed class InventoryAnalyticsSummary
{
    [Key]
    public Guid InventoryCardId { get; set; }

    public Card? InventoryCard { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentEstimatedValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CostBasis { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnrealizedGainLoss { get; set; }

    [Column(TypeName = "decimal(9,4)")]
    public decimal ROI { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal PriceConfidence { get; set; }

    [StringLength(32)]
    public string PriceFreshness { get; set; } = "Unknown";

    [Column(TypeName = "decimal(5,2)")]
    public decimal GradingScore { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal SellScore { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal HoldScore { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal LiquidityScore { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal DemandScore { get; set; }

    [StringLength(64)]
    public string Recommendation { get; set; } = "Insufficient data";

    [StringLength(500)]
    public string RecommendationReason { get; set; } = string.Empty;

    public DateTime LastCalculatedAt { get; set; }
}

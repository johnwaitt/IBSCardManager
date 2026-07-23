using System.ComponentModel.DataAnnotations;

namespace IBSCardManager.Entities;

public sealed class UserAnalyticsPreference
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [StringLength(100)]
    public string ProfileName { get; set; } = "Default";

    public bool NotifyPriceIncrease { get; set; } = true;
    public bool NotifyPriceDecrease { get; set; } = true;
    public bool NotifyGradingOpportunity { get; set; } = true;
    public bool NotifyStaleValuation { get; set; } = true;
    public bool NotifyReadyToList { get; set; } = true;
    public bool NotifyListingSold { get; set; } = true;
    public bool NotifyDuplicateInventory { get; set; } = true;
    public bool NotifyMissingCostBasis { get; set; } = true;
    public bool NotifyLowConfidenceIdentification { get; set; } = true;

    [StringLength(64)]
    public string CollectionGoal { get; set; } = "MaximizeProfit";

    public decimal MinimumCashFlowTarget { get; set; }
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}

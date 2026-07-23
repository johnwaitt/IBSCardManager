namespace IBSCardManager.Models;

public sealed class AnalyticsPreferenceViewModel
{
    public Guid Id { get; init; }
    public string ProfileName { get; init; } = "Default";
    public string CollectionGoal { get; init; } = "MaximizeProfit";
    public decimal MinimumCashFlowTarget { get; init; }
    public bool NotifyPriceIncrease { get; init; }
    public bool NotifyPriceDecrease { get; init; }
    public bool NotifyGradingOpportunity { get; init; }
    public bool NotifyStaleValuation { get; init; }
    public bool NotifyReadyToList { get; init; }
    public bool NotifyListingSold { get; init; }
    public bool NotifyDuplicateInventory { get; init; }
    public bool NotifyMissingCostBasis { get; init; }
    public bool NotifyLowConfidenceIdentification { get; init; }
    public DateTime ModifiedAt { get; init; }
}

namespace IBSCardManager.Models;

public class CollectionOverviewViewModel
{
    public string Search { get; set; } = string.Empty;
    public int TotalSets { get; set; }
    public int TotalChecklistCards { get; set; }
    public int TotalUniqueOwned { get; set; }
    public int TotalPiecesOwned { get; set; }
    public List<CollectionSetSummary> Sets { get; set; } = new();
}

public class CollectionSetSummary
{
    public Guid ProductId { get; set; }
    public int Year { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public int ChecklistCount { get; set; }
    public int UniqueOwned { get; set; }
    public int TotalQuantity { get; set; }
    public int MissingCount => Math.Max(0, ChecklistCount - UniqueOwned);
    public decimal CompletionPercent => ChecklistCount == 0 ? 0 : Math.Round((decimal)UniqueOwned / ChecklistCount * 100m, 1);
    public string TcdbSearchUrl => $"https://www.google.com/search?q={Uri.EscapeDataString($"site:tcdb.com {DisplayName} checklist")}";
}

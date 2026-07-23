namespace IBSCardManager.Models;

public sealed class CollectionBuilderViewModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductEdition { get; set; }
    public int? Year { get; set; }
    public string? Manufacturer { get; set; }
    public string? Brand { get; set; }
    public int TotalChecklistCards { get; set; }
    public int UniqueOwned { get; set; }
    public int TotalPiecesOwned { get; set; }
    public int MissingCount { get; set; }
    public decimal CompletionPercent { get; set; }
    public string? EmptyMessage { get; set; }
    public string? ErrorMessage { get; set; }
    public bool HasChecklistRows { get; set; }
    public string ChecklistStatusLabel { get; set; } = ChecklistStatus.ChecklistUnavailable.ToLabel();
    public string? LastImportSource { get; set; }
    public List<CollectionBuilderRow> Rows { get; set; } = new();
}

public sealed class CollectionBuilderRow
{
    public Guid ChecklistItemId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ChecklistSectionId { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Team { get; set; }
    public string? TeamSummary { get; set; }
    public string? ChecklistSection { get; set; }
    public string? Parallel { get; set; }
    public string? Variation { get; set; }
    public bool IsRookie { get; set; }
    public bool IsAutograph { get; set; }
    public bool IsRelic { get; set; }
    public string? ReferenceImageUrl { get; set; }
    public string? StockImageUrl { get; set; }
    public string ImageChoice { get; set; } = "Use reference image";
    public int ExistingQuantity { get; set; }
    public int QuantityToAdd { get; set; }
    public bool UseStockImage { get; set; } = true;
    public bool UseReferenceImage { get; set; } = true;
    public string SortKey => $"{ChecklistSection ?? string.Empty}|{CardNumber}|{Subject}";
}

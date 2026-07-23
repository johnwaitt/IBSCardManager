namespace IBSCardManager.Models;

public sealed class ChecklistGridViewModel
{
    public Guid? ProductId { get; set; }
    public string? Search { get; set; }
    public string? ErrorMessage { get; set; }
    public string? EmptyMessage { get; set; }
    public string? ProductName { get; set; }
    public string ChecklistStatusLabel { get; set; } = ChecklistStatus.ChecklistUnavailable.ToLabel();
    public string? LastImportSource { get; set; }
    public int TotalCount { get; set; }
    public int FilteredCount { get; set; }
    public int CardsWithPlayers { get; set; }
    public int CardsWithTeams { get; set; }
    public int CardsWithReferenceImages { get; set; }
    public int ChecklistSectionCount { get; set; }
    public List<ChecklistGridRowViewModel> Rows { get; set; } = new();
    public List<ChecklistProductOptionViewModel> Products { get; set; } = new();
}

public sealed class ChecklistGridRowViewModel
{
    public Guid ChecklistItemId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductDisplayName { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string Player { get; set; } = string.Empty;
    public string? Team { get; set; }
    public string? CardType { get; set; }
    public string? ChecklistSection { get; set; }
    public string? Parallel { get; set; }
    public string? Variation { get; set; }
    public bool IsRookie { get; set; }
    public bool IsAutograph { get; set; }
    public bool IsRelic { get; set; }
    public int? SerialMaximum { get; set; }
    public int OwnedQuantity { get; set; }
    public string? ReferenceImageUrl { get; set; }
}

public sealed class ChecklistProductOptionViewModel
{
    public Guid ProductId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string ChecklistStatusLabel { get; set; } = ChecklistStatus.ChecklistUnavailable.ToLabel();
    public int ChecklistCardCount { get; set; }
    public int ChecklistSectionCount { get; set; }
    public int PlayerLinkCount { get; set; }
    public int TeamLinkCount { get; set; }
    public int ReferenceImageCount { get; set; }
    public string? LastImportSource { get; set; }
}

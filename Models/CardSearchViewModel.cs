namespace IBSCardManager.Models;

public class CardSearchViewModel
{
    public string? Query { get; set; }
    public int? Year { get; set; }
    public string? Player { get; set; }
    public string? Team { get; set; }
    public string? Set { get; set; }
    public string? CardNumber { get; set; }
    public List<CardSearchResult> Results { get; set; } = new();
    public List<OnlineSearchLink> OnlineLinks { get; set; } = new();
}

public class CardSearchResult
{
    public string Source { get; set; } = string.Empty;
    public Guid RecordId { get; set; }
    public Guid? ProductId { get; set; }
    public string Player { get; set; } = string.Empty;
    public string? Team { get; set; }
    public int? Year { get; set; }
    public string? Set { get; set; }
    public string? CardNumber { get; set; }
    public string? Variety { get; set; }
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    public int Score { get; set; }
    public bool IsRookie { get; set; }
    public bool IsAutograph { get; set; }
    public bool IsRelic { get; set; }
}

public class OnlineSearchLink
{
    public string Provider { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

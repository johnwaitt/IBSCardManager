namespace IBSCardManager.Models;

public class AiCardLookupResult
{
    public string? Subject { get; set; }
    public string? Team { get; set; }
    public int? Year { get; set; }
    public string? Set { get; set; }
    public string? CardNumber { get; set; }
    public string? Variety { get; set; }
    public string? Serial { get; set; }
    public string Category { get; set; } = "Baseball";
    public bool IsRookie { get; set; }
    public bool IsAutograph { get; set; }
    public bool IsRelic { get; set; }
    public decimal Confidence { get; set; }
    public string? Notes { get; set; }
}

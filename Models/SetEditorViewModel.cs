namespace IBSCardManager.Models;

public class SetEditorViewModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public List<SetEditorRow> Rows { get; set; } = new();
}

public class SetEditorRow
{
    public Guid ChecklistItemId { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Team { get; set; }
    public string? Position { get; set; }
    public string? Subset { get; set; }
    public string? Parallel { get; set; }
    public string? Variation { get; set; }
    public string? SerialNumber { get; set; }
    public string? PrintRun { get; set; }
    public bool IsRookie { get; set; }
    public bool IsAutograph { get; set; }
    public bool IsRelic { get; set; }
    public bool IsRefractor { get; set; }
    public string? StockImageUrl { get; set; }
    public string? StockBackImageUrl { get; set; }

    public int Quantity { get; set; }
    public decimal? ListingPrice { get; set; }
    public string? GradeIssuer { get; set; }
    public string? Grade { get; set; }
    public string? CertNumber { get; set; }
    public string? EbaySku { get; set; }
    public string? EbayTitle { get; set; }
    public string? EbayDescription { get; set; }
    public string? EbayCategoryId { get; set; }
    public string? EbayCondition { get; set; }
    public string ListingFormat { get; set; } = "FixedPrice";
    public bool BestOfferEnabled { get; set; }
    public string? ShippingPolicyName { get; set; }
    public string? ReturnPolicyName { get; set; }
    public string? PaymentPolicyName { get; set; }
    public decimal? PackageWeightOz { get; set; }
    public decimal? PackageLengthIn { get; set; }
    public decimal? PackageWidthIn { get; set; }
    public decimal? PackageHeightIn { get; set; }
}

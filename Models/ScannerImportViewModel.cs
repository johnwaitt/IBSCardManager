using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IBSCardManager.Models;

public class ScannerImportViewModel
{
    public List<ScannerImageItem> Images { get; set; } = new();
    public List<SelectListItem> Products { get; set; } = new();

    [Display(Name = "Front Image")]
    public string? FrontFileName { get; set; }

    [Display(Name = "Back Image")]
    public string? BackFileName { get; set; }

    [Display(Name = "Player Name")]
    public string? Subject { get; set; }

    public string? Team { get; set; }
    public int? Year { get; set; }

    [Display(Name = "Sport")]
    public string Category { get; set; } = "Baseball";

    [Display(Name = "Set / Product")]
    public string? Set { get; set; }

    [Display(Name = "Card Number")]
    public string? CardNumber { get; set; }

    [Display(Name = "Parallel / Variation")]
    public string? Variety { get; set; }

    public string? Serial { get; set; }
    public bool IsRookie { get; set; }
    public bool IsAutograph { get; set; }
    public bool IsRelic { get; set; }
    public int Quantity { get; set; } = 1;

    [Display(Name = "Stock Image URL")]
    public string? StockImageUrl { get; set; }

    [Display(Name = "Preferred Image")]
    public string PreferredImageSource { get; set; } = "Scan";

    [Display(Name = "Destination")]
    public string Destination { get; set; } = "Inventory";

    [Display(Name = "Catalog Set")]
    public Guid? ProductId { get; set; }

    public Guid? MatchedChecklistItemId { get; set; }
    public Guid? MatchedInventoryCardId { get; set; }
    public Guid? ConfirmedChecklistItemId { get; set; }
    public string? ScannerPairId { get; set; }
    public string? ScannerPairState { get; set; }
    public string? FrontImageHash { get; set; }
    public string? BackImageHash { get; set; }
    public string? CombinedImageHash { get; set; }
    public decimal? IdentificationConfidence { get; set; }
    public string? CandidateSource { get; set; }
    public bool EnableChatGptAnalysis { get; set; }
    public bool AskBeforeChatGptUpload { get; set; } = true;
    public bool EnableWebLookup { get; set; } = true;
    public bool AskBeforeWebLookup { get; set; } = true;
    public bool LocalAnalysisOnly { get; set; }
    public bool AllowTextOnlyOnlineSearch { get; set; } = true;
    public bool ReuseCachedAnalysis { get; set; } = true;
    public string VisionModel { get; set; } = string.Empty;
    public string SearchProvider { get; set; } = "None";
    public decimal LocalConfidenceThresholdPercent { get; set; } = 85m;
    public int TimeoutSeconds { get; set; } = 60;
    public int MaxRetries { get; set; } = 2;
    public int MaxImageBytes { get; set; } = 8 * 1024 * 1024;
    public string? Message { get; set; }

    public bool HasUserCorrections { get; set; }
    public string? CorrectionFieldName { get; set; }
    public string? CorrectionType { get; set; }
    public string? CorrectionOriginalValue { get; set; }
    public string? CorrectionCorrectedValue { get; set; }
    public string? CorrectionReason { get; set; }
    public string? CorrectionNotes { get; set; }
}

public class ScannerImageItem
{
    public string FileName { get; set; } = string.Empty;
    public string PreviewUrl { get; set; } = string.Empty;
    public DateTime ModifiedDate { get; set; }
    public long SizeBytes { get; set; }
}

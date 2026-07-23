namespace IBSCardManager.Models;

public sealed class WebChecklistSearchRequest
{
    public Guid? ProductId { get; set; }
    public int? Year { get; set; }
    public string? Manufacturer { get; set; }
    public string? Brand { get; set; }
    public string? Product { get; set; }
    public string? Edition { get; set; }
    public string? Sport { get; set; }
}

public sealed class WebCardSearchRequest
{
    public Guid? ProductId { get; set; }
    public string? Player { get; set; }
    public int? Year { get; set; }
    public string? Manufacturer { get; set; }
    public string? Product { get; set; }
    public string? CardNumber { get; set; }
    public string? Team { get; set; }
    public string? Parallel { get; set; }
    public string? Variation { get; set; }
}

public sealed class WebSearchCandidateViewModel
{
    public string? Title { get; set; }
    public string? Snippet { get; set; }
    public string? PageSource { get; set; }
    public string? PageUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string? SourceDomain { get; set; }
    public string SearchQuery { get; set; } = string.Empty;
    public DateTime DateRetrievedUtc { get; set; } = DateTime.UtcNow;
    public bool UserConfirmed { get; set; }
    public string Classification { get; set; } = "Unknown";
    public decimal SourceConfidencePercent { get; set; }
    public string? Player { get; set; }
    public int? Year { get; set; }
    public string? Manufacturer { get; set; }
    public string? Brand { get; set; }
    public string? Product { get; set; }
    public string? CardNumber { get; set; }
    public string? Team { get; set; }
    public string? Parallel { get; set; }
    public string? Variation { get; set; }
    public IReadOnlyList<string> MatchReasons { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Conflicts { get; set; } = Array.Empty<string>();
}

public sealed class ChecklistImportPreviewRequest
{
    public Guid ProductId { get; set; }
    public string SourceName { get; set; } = "User Import";
    public string SourceType { get; set; } = "UserUpload";
    public IReadOnlyList<ChecklistImportInputRow> Rows { get; set; } = Array.Empty<ChecklistImportInputRow>();
}

public sealed class ChecklistImportInputRow
{
    public string CardNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Team { get; set; }
    public string? ChecklistSection { get; set; }
    public string? Parallel { get; set; }
    public string? Variation { get; set; }
    public string? SourceRecordId { get; set; }
    public string? Notes { get; set; }
}

public sealed class ChecklistImportPreviewRowViewModel
{
    public int RowNumber { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Team { get; set; }
    public string? ChecklistSection { get; set; }
    public string? Parallel { get; set; }
    public string? Variation { get; set; }
    public string? SourceRecordId { get; set; }
    public string? Notes { get; set; }
    public string ValidationStatus { get; set; } = "Ready";
}

public sealed class ReferenceImageMetadataRequest
{
    public string? ReferenceImageUrl { get; set; }
    public string? ReferencePageUrl { get; set; }
    public string? ImageSource { get; set; }
    public DateTime? DateLocatedUtc { get; set; }
    public string? UsageStatus { get; set; }
    public string? CachedThumbnailPath { get; set; }
    public string? ImageHash { get; set; }
    public string? VerificationStatus { get; set; }
}

public sealed class ReferenceImageMetadataViewModel
{
    public string? ReferenceImageUrl { get; set; }
    public string? ReferencePageUrl { get; set; }
    public string? ImageSource { get; set; }
    public DateTime DateLocatedUtc { get; set; }
    public string? UsageStatus { get; set; }
    public string? CachedThumbnailPath { get; set; }
    public string? ImageHash { get; set; }
    public string? VerificationStatus { get; set; }
}

public sealed class ScannerStructuredSearchRequest
{
    public string? Player { get; set; }
    public int? Year { get; set; }
    public string? Product { get; set; }
    public string? CardNumber { get; set; }
    public string? Team { get; set; }
    public string? Parallel { get; set; }
    public string? Variation { get; set; }
}

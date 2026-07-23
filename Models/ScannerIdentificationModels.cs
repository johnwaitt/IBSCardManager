using System.Security.Cryptography;
using System.Text;

namespace IBSCardManager.Models;

public static class ScannerFieldSources
{
    public const string Front = "Front";
    public const string Back = "Back";
    public const string Both = "Both";
    public const string Unknown = "Unknown";
    public const string UserHint = "User hint";
    public const string UserCorrected = "User corrected";
}

public sealed class ScannerIdentificationRequest
{
    public string? FrontFileName { get; init; }
    public string? BackFileName { get; init; }
    public string? FrontPath { get; init; }
    public string? BackPath { get; init; }
    public ScannerExtractionHints Hints { get; init; } = new();
}

public sealed class ScannerExtractionHints
{
    public string? Player { get; init; }
    public string? Team { get; init; }
    public int? Year { get; init; }
    public string? Manufacturer { get; init; }
    public string? Brand { get; init; }
    public string? Product { get; init; }
    public string? CardNumber { get; init; }
    public string? ChecklistSection { get; init; }
    public string? Parallel { get; init; }
    public string? Variation { get; init; }
    public string? SerialNumber { get; init; }
    public int? SerialMaximum { get; init; }
    public bool? IsRookie { get; init; }
    public bool? IsAutograph { get; init; }
    public bool? IsRelic { get; init; }
    public bool? IsPatch { get; init; }
}

public sealed class ScannerIdentificationResult
{
    public ScannerExtractionField Player { get; set; } = new();
    public ScannerExtractionField Team { get; set; } = new();
    public ScannerExtractionField Year { get; set; } = new();
    public ScannerExtractionField Manufacturer { get; set; } = new();
    public ScannerExtractionField Brand { get; set; } = new();
    public ScannerExtractionField Product { get; set; } = new();
    public ScannerExtractionField ProductEdition { get; set; } = new();
    public ScannerExtractionField CardNumber { get; set; } = new();
    public ScannerExtractionField ChecklistSection { get; set; } = new();
    public ScannerExtractionField Parallel { get; set; } = new();
    public ScannerExtractionField Variation { get; set; } = new();
    public ScannerExtractionField Rookie { get; set; } = new();
    public ScannerExtractionField Autograph { get; set; } = new();
    public ScannerExtractionField Relic { get; set; } = new();
    public ScannerExtractionField Patch { get; set; } = new();
    public ScannerExtractionField ShortPrint { get; set; } = new();
    public ScannerExtractionField SerialNumber { get; set; } = new();
    public ScannerExtractionField SerialMaximum { get; set; } = new();
    public ScannerExtractionField CopyrightYear { get; set; } = new();
    public ScannerExtractionField VisibleText { get; set; } = new();
    public ScannerExtractionField Orientation { get; set; } = new();
    public ScannerExtractionField SamePhysicalCard { get; set; } = new();
    public decimal OverallConfidence { get; set; }
    public bool UsedCachedAnalysis { get; set; }
    public IReadOnlyList<string> Warnings { get; set; } = Array.Empty<string>();
    public IReadOnlyList<CardAnalysisEvidence> Evidence { get; set; } = Array.Empty<CardAnalysisEvidence>();
    public IReadOnlyList<ScannerCandidateResult> Candidates { get; set; } = Array.Empty<ScannerCandidateResult>();
    public string? Notes { get; set; }
}

public sealed class ScannerExtractionField
{
    public string? Value { get; init; }
    public string? NormalizedValue { get; init; }
    public decimal Confidence { get; init; }
    public string Source { get; init; } = ScannerFieldSources.Unknown;
    public bool IsUncertain => Confidence > 0m && Confidence < 0.7m;
}

public sealed class ScannerCandidateResult
{
    public Guid? ChecklistItemId { get; init; }
    public Guid? ProductId { get; init; }
    public Guid? InventoryCardId { get; init; }
    public string CatalogSource { get; init; } = "Checklist";
    public string MatchStatus { get; init; } = "Weak Match";
    public string Player { get; init; } = string.Empty;
    public string? Team { get; init; }
    public int? Year { get; init; }
    public string? Manufacturer { get; init; }
    public string? Brand { get; init; }
    public string? Product { get; init; }
    public string? ChecklistSection { get; init; }
    public string? CardNumber { get; init; }
    public string? Parallel { get; init; }
    public string? Variation { get; init; }
    public bool IsRookie { get; init; }
    public bool IsAutograph { get; init; }
    public bool IsRelic { get; init; }
    public bool IsPatch { get; init; }
    public int? SerialMaximum { get; init; }
    public string? ReferenceImageUrl { get; init; }
    public decimal Confidence { get; init; }
    public IReadOnlyList<string> MatchReasons { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Conflicts { get; init; } = Array.Empty<string>();
}

public sealed class ScannerSelectedCatalogCardDto
{
    public Guid? ProductId { get; init; }
    public Guid? ChecklistCardId { get; init; }
    public Guid? MasterCardId { get; init; }
    public Guid? PlayerSubjectId { get; init; }
    public Guid? TeamId { get; init; }
    public string? Player { get; init; }
    public string? Team { get; init; }
    public int? Year { get; init; }
    public string? Manufacturer { get; init; }
    public string? Brand { get; init; }
    public string? Product { get; init; }
    public string? ChecklistSection { get; init; }
    public string? CardNumber { get; init; }
    public string? Parallel { get; init; }
    public string? Variation { get; init; }
    public bool IsRookie { get; init; }
    public bool IsAutograph { get; init; }
    public bool IsRelic { get; init; }
    public bool IsPatch { get; init; }
    public int? SerialMaximum { get; init; }
    public string CandidateSource { get; init; } = "local-catalog";
}

public sealed class ScannerComparisonRow
{
    public string Field { get; init; } = string.Empty;
    public string? ExtractedValue { get; init; }
    public string? CatalogValue { get; init; }
    public string Result { get; init; } = "Unknown";
}

public sealed class ScannerDuplicateCheckRequest
{
    public Guid? ChecklistCardId { get; init; }
    public Guid? ProductId { get; init; }
    public string? CardNumber { get; init; }
    public string? Player { get; init; }
    public string? Parallel { get; init; }
    public string? Variation { get; init; }
    public string? FrontFileName { get; init; }
    public string? BackFileName { get; init; }
    public string? FrontPath { get; init; }
    public string? BackPath { get; init; }
    public string? PairId { get; init; }
    public string? PairState { get; init; }
}

public sealed class ScannerDuplicateCheckResult
{
    public IReadOnlyList<ScannerDuplicateWarning> Warnings { get; init; } = Array.Empty<ScannerDuplicateWarning>();
    public bool HasLikelyDuplicate => Warnings.Count > 0;
}

public sealed class ScannerPrivacyOptions
{
    public bool AllowExternalTextLookup { get; set; }
    public bool AllowExternalImageAnalysis { get; set; }
    public bool AskEveryTime { get; set; } = true;
    public bool RememberLastChoice { get; set; }
    public bool LocalAnalysisOnly { get; set; } = true;
}

public sealed class ScannerAiSettingsInput
{
    public bool EnableChatGptAnalysis { get; set; }
    public bool AskBeforeUpload { get; set; } = true;
    public bool EnableWebLookup { get; set; } = true;
    public bool AskBeforeWebLookup { get; set; } = true;
    public bool LocalAnalysisOnly { get; set; }
    public bool AllowTextOnlyOnlineSearch { get; set; } = true;
    public bool ReuseCachedAnalysis { get; set; } = true;
    public string? VisionModel { get; set; }
    public string? SearchProvider { get; set; }
    public decimal LocalConfidenceThresholdPercent { get; set; } = 85m;
    public int TimeoutSeconds { get; set; } = 60;
    public int MaxRetries { get; set; } = 2;
    public int MaxImageBytes { get; set; } = 8 * 1024 * 1024;
}

public sealed class ScannerDuplicateWarning
{
    public string Category { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Guid? ExistingCardId { get; init; }
    public Guid? ChecklistItemId { get; init; }
    public bool IsExactMatch { get; init; }
}

public static class ScannerImageHashUtility
{
    public static string? ComputeHash(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
        using var stream = File.OpenRead(path);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }

    public static string? ComputeCombinedHash(string? frontPath, string? backPath)
    {
        var front = ComputeHash(frontPath);
        var back = ComputeHash(backPath);
        if (front == null && back == null) return null;
        var combined = $"{front ?? string.Empty}|{back ?? string.Empty}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return Convert.ToHexString(bytes);
    }
}


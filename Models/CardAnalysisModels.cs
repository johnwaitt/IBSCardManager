namespace IBSCardManager.Models;

public static class CardEvidenceSource
{
    public const string Front = "front";
    public const string Back = "back";
    public const string Both = "both";
    public const string Unknown = "unknown";
    public const string UserCorrected = "user-corrected";
}

public sealed class CardFieldResult<T>
{
    public T? Value { get; set; }
    public decimal Confidence { get; set; }
    public string EvidenceSource { get; set; } = CardEvidenceSource.Unknown;
}

public sealed class CardAnalysisEvidence
{
    public string Field { get; set; } = string.Empty;
    public string? Snippet { get; set; }
    public string Source { get; set; } = CardEvidenceSource.Unknown;
}

public sealed class CardAnalysisWarning
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class CardAnalysisResult
{
    public CardFieldResult<string> PlayerName { get; set; } = new();
    public List<CardFieldResult<string>> AdditionalSubjects { get; set; } = new();
    public CardFieldResult<string> Team { get; set; } = new();
    public CardFieldResult<string> Sport { get; set; } = new();
    public CardFieldResult<int?> Year { get; set; } = new();
    public CardFieldResult<string> Manufacturer { get; set; } = new();
    public CardFieldResult<string> Brand { get; set; } = new();
    public CardFieldResult<string> Product { get; set; } = new();
    public CardFieldResult<string> ProductEdition { get; set; } = new();
    public CardFieldResult<string> ChecklistSection { get; set; } = new();
    public CardFieldResult<string> CardNumber { get; set; } = new();
    public CardFieldResult<string> Parallel { get; set; } = new();
    public CardFieldResult<string> Variation { get; set; } = new();
    public CardFieldResult<bool?> Rookie { get; set; } = new();
    public CardFieldResult<bool?> Autograph { get; set; } = new();
    public CardFieldResult<bool?> Relic { get; set; } = new();
    public CardFieldResult<bool?> Patch { get; set; } = new();
    public CardFieldResult<bool?> ShortPrint { get; set; } = new();
    public CardFieldResult<string> SerialNumber { get; set; } = new();
    public CardFieldResult<int?> SerialMaximum { get; set; } = new();
    public CardFieldResult<int?> PrintedCopyrightYear { get; set; } = new();
    public CardFieldResult<string> FrontText { get; set; } = new();
    public CardFieldResult<string> BackText { get; set; } = new();
    public CardFieldResult<bool?> SamePhysicalCard { get; set; } = new();
    public decimal Confidence { get; set; }
    public List<CardAnalysisWarning> Warnings { get; set; } = new();
    public List<CardAnalysisEvidence> Evidence { get; set; } = new();
}

public sealed class CardAnalysisHints
{
    public string? PlayerName { get; set; }
    public string? Team { get; set; }
    public int? Year { get; set; }
    public string? Product { get; set; }
    public string? CardNumber { get; set; }
    public string? Parallel { get; set; }
    public string? Variation { get; set; }
}

public sealed class CardAnalysisRequest
{
    public string FrontImagePath { get; set; } = string.Empty;
    public string? BackImagePath { get; set; }
    public string? PairId { get; set; }
    public string? FrontImageHash { get; set; }
    public string? BackImageHash { get; set; }
    public CardAnalysisHints Hints { get; set; } = new();
}

public sealed class CardAnalysisResponseEnvelope
{
    public CardAnalysisResult? Analysis { get; set; }
    public bool Cached { get; set; }
    public string? Model { get; set; }
    public TimeSpan Duration { get; set; }
    public int? ResponseFieldCount { get; set; }
}
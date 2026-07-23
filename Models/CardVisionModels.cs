namespace IBSCardManager.Models;

public sealed class CardVisionFieldResult<T>
{
    public T? Value { get; set; }
    public decimal Confidence { get; set; }
    public string EvidenceSource { get; set; } = CardEvidenceSource.Unknown;
}

public sealed class CardVisionEvidence
{
    public string Field { get; set; } = string.Empty;
    public string? Snippet { get; set; }
    public string Source { get; set; } = CardEvidenceSource.Unknown;
}

public sealed class CardVisionWarning
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class CardVisionAnalysisResult
{
    public CardVisionFieldResult<string> PlayerName { get; set; } = new();
    public List<CardVisionFieldResult<string>> AdditionalSubjects { get; set; } = new();
    public CardVisionFieldResult<string> Team { get; set; } = new();
    public CardVisionFieldResult<string> Sport { get; set; } = new();
    public CardVisionFieldResult<int?> Year { get; set; } = new();
    public CardVisionFieldResult<string> Manufacturer { get; set; } = new();
    public CardVisionFieldResult<string> Brand { get; set; } = new();
    public CardVisionFieldResult<string> Product { get; set; } = new();
    public CardVisionFieldResult<string> ProductEdition { get; set; } = new();
    public CardVisionFieldResult<string> ChecklistSection { get; set; } = new();
    public CardVisionFieldResult<string> CardNumber { get; set; } = new();
    public CardVisionFieldResult<string> Parallel { get; set; } = new();
    public CardVisionFieldResult<string> Variation { get; set; } = new();
    public CardVisionFieldResult<bool?> Rookie { get; set; } = new();
    public CardVisionFieldResult<bool?> Autograph { get; set; } = new();
    public CardVisionFieldResult<bool?> Relic { get; set; } = new();
    public CardVisionFieldResult<bool?> Patch { get; set; } = new();
    public CardVisionFieldResult<bool?> ShortPrint { get; set; } = new();
    public CardVisionFieldResult<string> SerialNumber { get; set; } = new();
    public CardVisionFieldResult<int?> SerialMaximum { get; set; } = new();
    public CardVisionFieldResult<int?> PrintedCopyrightYear { get; set; } = new();
    public CardVisionFieldResult<string> FrontText { get; set; } = new();
    public CardVisionFieldResult<string> BackText { get; set; } = new();
    public CardVisionFieldResult<bool?> SamePhysicalCard { get; set; } = new();
    public decimal OverallConfidence { get; set; }
    public List<CardVisionWarning> Warnings { get; set; } = new();
    public List<CardVisionEvidence> FieldEvidence { get; set; } = new();
}

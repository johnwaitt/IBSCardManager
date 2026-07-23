namespace IBSCardManager.Models;

public sealed class AdministrationUpdatesViewModel
{
    public string ApplicationVersion { get; init; } = string.Empty;
    public string InformationalVersion { get; init; } = string.Empty;
    public string? DatabaseVersion { get; init; }
    public string? CatalogVersion { get; init; }
    public string? KnowledgeVersion { get; init; }
    public string? PromptVersion { get; init; }
    public string? RuleVersion { get; init; }
    public string? MarketplaceVersion { get; init; }
    public string? AiModelVersion { get; init; }
}

public sealed class AdministrationFeatureManagerViewModel
{
    public bool DeveloperModeEnabled { get; init; }
    public IReadOnlyDictionary<string, bool> FeatureFlags { get; init; } = new Dictionary<string, bool>();
}

public sealed class AdministrationAuditEntryViewModel
{
    public DateTimeOffset Timestamp { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
}

public sealed class AdministrationAuditHistoryViewModel
{
    public IReadOnlyList<AdministrationAuditEntryViewModel> Entries { get; init; } = Array.Empty<AdministrationAuditEntryViewModel>();
}

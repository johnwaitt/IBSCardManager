using IBSCardManager.Services;

namespace IBSCardManager.Models;

public sealed class DiagnosticsViewModel
{
    public string ApplicationVersion { get; init; } = string.Empty;
    public string InformationalVersion { get; init; } = string.Empty;
    public string UpperLeftDisplayVersion { get; init; } = string.Empty;
    public string? SchemaVersion { get; init; }
    public string? CatalogVersion { get; init; }
    public string CatalogProvider { get; init; } = "Unknown";
    public string CatalogDatabaseStatus { get; init; } = "Unknown";
    public string? KnowledgeSchemaVersion { get; init; }
    public string? ConfidenceRuleVersion { get; init; }
    public string? LearningRuleVersion { get; init; }
    public string? AiModelConfiguration { get; init; }
    public int KnowledgeRecordCount { get; init; }
    public int EvidenceCount { get; init; }
    public int CorrectionCount { get; init; }
    public int PendingReviewCount { get; init; }
    public int DisputedRecordCount { get; init; }
    public int LowConfidenceRecordCount { get; init; }
    public string KnowledgeHealthStatus { get; init; } = "Not Configured";
    public string? LastKnowledgeBackup { get; init; }
    public IReadOnlyList<CatalogIntegrityIssue> IntegrityIssues { get; init; } = Array.Empty<CatalogIntegrityIssue>();
    public DateTimeOffset GeneratedAt { get; init; }
}

namespace IBSCardManager.Models;

public sealed class BackupManifest
{
    public string ApplicationVersion { get; init; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; init; }
    public IReadOnlyList<BackupManifestEntry> Databases { get; init; } = Array.Empty<BackupManifestEntry>();
}

public sealed class BackupManifestEntry
{
    public string DatabaseName { get; init; } = string.Empty;
    public string DatabaseRole { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public string? SchemaVersion { get; init; }
    public string? CatalogVersion { get; init; }
    public string? KnowledgeSchemaVersion { get; init; }
    public DateTimeOffset BackupDate { get; init; }
    public long FileSizeBytes { get; init; }
    public string? ChecksumSha256 { get; init; }
    public bool VerificationResult { get; init; }
}

using System.Security.Cryptography;
using IBSCardManager.Data;
using IBSCardManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IBSCardManager.Services;

public sealed class BackupManifestService : IBackupManifestService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ICatalogVersionService _catalogVersionService;
    private readonly IApplicationVersionProvider _applicationVersionProvider;

    public BackupManifestService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ICatalogVersionService catalogVersionService,
        IApplicationVersionProvider applicationVersionProvider)
    {
        _context = context;
        _configuration = configuration;
        _catalogVersionService = catalogVersionService;
        _applicationVersionProvider = applicationVersionProvider;
    }

    public async Task<BackupManifest> GenerateManifestAsync(CancellationToken cancellationToken = default)
    {
        var schemaVersion = _context.Database.GetAppliedMigrations().LastOrDefault();
        var catalogVersion = await _catalogVersionService.GetCatalogVersionAsync(cancellationToken);

        var backupDirectory = Path.Combine(AppContext.BaseDirectory, "backups");
        Directory.CreateDirectory(backupDirectory);

        var latestBackup = Directory.EnumerateFiles(backupDirectory, "*.bak", SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .FirstOrDefault();

        var connection = _configuration.GetConnectionString("CardManagerConnection") ?? string.Empty;
        var dbName = ParseDatabaseName(connection) ?? "IBSCardManager";

        var entries = new List<BackupManifestEntry>();

        entries.Add(BuildEntry(dbName, "RuntimePrimary", latestBackup, schemaVersion, catalogVersion, null));

        var knowledgeBackup = Directory.EnumerateFiles(backupDirectory, "*knowledge*.bak", SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .FirstOrDefault();

        entries.Add(BuildEntry(dbName, "KnowledgeEngine", knowledgeBackup, schemaVersion, catalogVersion, KnowledgeModelVersionService.KnowledgeSchemaVersionValue));

        return new BackupManifest
        {
            ApplicationVersion = _applicationVersionProvider.ApplicationVersion,
            GeneratedAt = DateTimeOffset.UtcNow,
            Databases = entries
        };
    }

    private static BackupManifestEntry BuildEntry(string dbName, string role, FileInfo? backup, string? schemaVersion, string? catalogVersion, string? knowledgeSchemaVersion)
    {
        var path = backup?.FullName ?? string.Empty;
        var exists = backup is not null && backup.Exists && backup.Length > 0;

        return new BackupManifestEntry
        {
            DatabaseName = dbName,
            DatabaseRole = role,
            FilePath = path,
            SchemaVersion = schemaVersion,
            CatalogVersion = catalogVersion,
            KnowledgeSchemaVersion = knowledgeSchemaVersion,
            BackupDate = backup is null
                ? DateTimeOffset.MinValue
                : new DateTimeOffset(backup.LastWriteTimeUtc, TimeSpan.Zero),
            FileSizeBytes = backup?.Length ?? 0,
            ChecksumSha256 = exists ? ComputeSha256(path) : null,
            VerificationResult = exists
        };
    }

    private static string? ParseDatabaseName(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            if (part.StartsWith("Database=", StringComparison.OrdinalIgnoreCase))
            {
                return part.Substring("Database=".Length);
            }

            if (part.StartsWith("Initial Catalog=", StringComparison.OrdinalIgnoreCase))
            {
                return part.Substring("Initial Catalog=".Length);
            }
        }

        return null;
    }

    private static string ComputeSha256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }
}

using IBSCardManager.Models;

namespace IBSCardManager.Services;

public sealed class AdministrationCenterService : IAdministrationCenterService
{
    private readonly IApplicationVersionProvider _versionProvider;
    private readonly IDiagnosticsService _diagnosticsService;
    private readonly IBackupManifestService _backupManifestService;

    public AdministrationCenterService(
        IApplicationVersionProvider versionProvider,
        IDiagnosticsService diagnosticsService,
        IBackupManifestService backupManifestService)
    {
        _versionProvider = versionProvider;
        _diagnosticsService = diagnosticsService;
        _backupManifestService = backupManifestService;
    }

    public async Task<AdministrationDashboardViewModel> BuildDashboardAsync(CancellationToken cancellationToken = default)
    {
        var diagnostics = await _diagnosticsService.BuildDiagnosticsAsync(cancellationToken);
        var backup = await _backupManifestService.GenerateManifestAsync(cancellationToken);

        var lastBackup = backup.Databases
            .Where(x => x.BackupDate != DateTimeOffset.MinValue)
            .OrderByDescending(x => x.BackupDate)
            .FirstOrDefault();

        var cards = new List<AdministrationStatusCardViewModel>
        {
            new()
            {
                Title = "Application Version",
                Status = _versionProvider.ApplicationVersion,
                Detail = "Centralized version provider",
                LastEventLabel = "Build",
                LastEventValue = _versionProvider.InformationalVersion,
                Severity = "Info"
            },
            new()
            {
                Title = "Master Database Status",
                Status = diagnostics.CatalogDatabaseStatus,
                Detail = diagnostics.CatalogProvider,
                LastEventLabel = "Last Catalog Update",
                LastEventValue = diagnostics.CatalogVersion ?? "Unavailable",
                Severity = diagnostics.CatalogDatabaseStatus == "Connected" ? "Success" : "Warning"
            },
            new()
            {
                Title = "AI Engine Status",
                Status = diagnostics.AiModelConfiguration ?? "Unavailable",
                Detail = $"Knowledge Health: {diagnostics.KnowledgeHealthStatus}",
                LastEventLabel = "Last AI Connection",
                LastEventValue = diagnostics.GeneratedAt.ToLocalTime().ToString("g"),
                Severity = diagnostics.KnowledgeHealthStatus is "Healthy" or "Warning" ? "Info" : "Warning"
            },
            new()
            {
                Title = "Marketplace Status",
                Status = "Limited",
                Detail = "Provider platform initialized; marketplace providers staged in Administration.",
                LastEventLabel = "Last Marketplace Sync",
                LastEventValue = "Unavailable",
                Severity = "Info"
            },
            new()
            {
                Title = "Storage Status",
                Status = "Available",
                Detail = "Runtime storage and backup location accessible.",
                LastEventLabel = "Schema Version",
                LastEventValue = diagnostics.SchemaVersion ?? "Unavailable",
                Severity = "Success"
            },
            new()
            {
                Title = "Backup Status",
                Status = lastBackup?.VerificationResult == true ? "Verified" : "Warning",
                Detail = "Backup manifest includes runtime and knowledge records.",
                LastEventLabel = "Last Backup",
                LastEventValue = lastBackup?.BackupDate.ToLocalTime().ToString("g") ?? "Unavailable",
                Severity = lastBackup?.VerificationResult == true ? "Success" : "Warning"
            },
            new()
            {
                Title = "System Health",
                Status = diagnostics.KnowledgeHealthStatus,
                Detail = $"Integrity issues: {diagnostics.IntegrityIssues.Count}",
                LastEventLabel = "Warnings / Errors",
                LastEventValue = $"{diagnostics.IntegrityIssues.Count(x => x.Severity.Equals("Warning", StringComparison.OrdinalIgnoreCase))} / {diagnostics.IntegrityIssues.Count(x => x.Severity.Equals("Error", StringComparison.OrdinalIgnoreCase))}",
                Severity = diagnostics.KnowledgeHealthStatus == "Failed" ? "Danger" : diagnostics.KnowledgeHealthStatus == "Warning" ? "Warning" : "Success"
            }
        };

        return new AdministrationDashboardViewModel
        {
            ApplicationVersion = _versionProvider.ApplicationVersion,
            InformationalVersion = _versionProvider.InformationalVersion,
            Cards = cards,
            GeneratedAt = DateTimeOffset.UtcNow
        };
    }
}

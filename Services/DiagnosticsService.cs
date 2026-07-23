using IBSCardManager.Data;
using IBSCardManager.Entities;
using IBSCardManager.Models;
using IBSCardManager.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IBSCardManager.Services;

public interface IDiagnosticsService
{
    Task<DiagnosticsViewModel> BuildDiagnosticsAsync(CancellationToken cancellationToken = default);
}

public sealed class DiagnosticsService : IDiagnosticsService
{
    private readonly ApplicationDbContext _context;
    private readonly ICatalogVersionService _catalogVersionService;
    private readonly ICatalogDatabaseProvider _catalogProvider;
    private readonly ICatalogValidationService _catalogValidationService;
    private readonly IApplicationVersionProvider _applicationVersionProvider;
    private readonly IKnowledgeModelVersionService _knowledgeModelVersionService;
    private readonly IKnowledgeHealthService _knowledgeHealthService;
    private readonly OpenAiCardAnalysisOptions _openAiOptions;

    public DiagnosticsService(
        ApplicationDbContext context,
        ICatalogVersionService catalogVersionService,
        ICatalogDatabaseProvider catalogProvider,
        ICatalogValidationService catalogValidationService,
        IApplicationVersionProvider applicationVersionProvider,
        IKnowledgeModelVersionService knowledgeModelVersionService,
        IKnowledgeHealthService knowledgeHealthService,
        IOptions<OpenAiCardAnalysisOptions> openAiOptions)
    {
        _context = context;
        _catalogVersionService = catalogVersionService;
        _catalogProvider = catalogProvider;
        _catalogValidationService = catalogValidationService;
        _applicationVersionProvider = applicationVersionProvider;
        _knowledgeModelVersionService = knowledgeModelVersionService;
        _knowledgeHealthService = knowledgeHealthService;
        _openAiOptions = openAiOptions.Value;
    }

    public async Task<DiagnosticsViewModel> BuildDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        var schemaVersion = _context.Database.GetAppliedMigrations().LastOrDefault();
        var catalogVersion = await _catalogVersionService.GetCatalogVersionAsync(cancellationToken);
        var canConnect = await _catalogProvider.CanConnectAsync(cancellationToken);
        var issues = await _catalogValidationService.RunReadinessChecksAsync(cancellationToken);
        var versionInfo = _knowledgeModelVersionService.GetCurrentVersions();
        var health = await _knowledgeHealthService.RunHealthChecksAsync(cancellationToken);

        var backupDirectory = Path.Combine(AppContext.BaseDirectory, "backups");
        var latestKnowledgeBackup = Directory.Exists(backupDirectory)
            ? Directory.EnumerateFiles(backupDirectory, "*knowledge*.bak", SearchOption.TopDirectoryOnly)
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .FirstOrDefault()
            : null;

        return new DiagnosticsViewModel
        {
            ApplicationVersion = _applicationVersionProvider.ApplicationVersion,
            InformationalVersion = _applicationVersionProvider.InformationalVersion,
            UpperLeftDisplayVersion = $"Ver {_applicationVersionProvider.ApplicationVersion}",
            SchemaVersion = schemaVersion,
            CatalogVersion = catalogVersion,
            CatalogProvider = _catalogProvider.ProviderName,
            CatalogDatabaseStatus = canConnect ? "Connected" : "Unavailable",
            KnowledgeSchemaVersion = versionInfo.KnowledgeSchemaVersion,
            ConfidenceRuleVersion = versionInfo.ConfidenceRuleVersion,
            LearningRuleVersion = versionInfo.LearningRuleVersion,
            AiModelConfiguration = string.IsNullOrWhiteSpace(versionInfo.AiModelName)
                ? "Not configured"
                : $"{versionInfo.AiModelName} (prompt {versionInfo.PromptTemplateVersion ?? "unknown"})",
            KnowledgeRecordCount = await _context.KnowledgeRecords.AsNoTracking().CountAsync(cancellationToken),
            EvidenceCount = await _context.KnowledgeEvidence.AsNoTracking().CountAsync(cancellationToken),
            CorrectionCount = await _context.UserCorrections.AsNoTracking().CountAsync(cancellationToken),
            PendingReviewCount = await _context.KnowledgeReviewItems.AsNoTracking().CountAsync(x => x.Status == KnowledgeReviewQueueState.New || x.Status == KnowledgeReviewQueueState.InReview, cancellationToken),
            DisputedRecordCount = await _context.KnowledgeRecords.AsNoTracking().CountAsync(x => x.VerificationLevel == KnowledgeVerificationLevel.Disputed, cancellationToken),
            LowConfidenceRecordCount = await _context.KnowledgeRecords.AsNoTracking().CountAsync(x => x.ConfidenceScore < 50m, cancellationToken),
            KnowledgeHealthStatus = health.OverallState.ToString(),
            LastKnowledgeBackup = latestKnowledgeBackup?.FullName,
            IntegrityIssues = issues,
            GeneratedAt = DateTimeOffset.UtcNow
        };
    }
}

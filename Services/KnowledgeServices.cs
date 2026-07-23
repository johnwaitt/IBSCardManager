using System.Text.Json;
using IBSCardManager.Data;
using IBSCardManager.Entities;
using IBSCardManager.Models;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Services;

public sealed class KnowledgeService : IKnowledgeService
{
    private readonly ApplicationDbContext _context;

    public KnowledgeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<KnowledgeRecord> UpsertKnowledgeRecordAsync(KnowledgeRecord record, CancellationToken cancellationToken = default)
    {
        var existing = await _context.KnowledgeRecords.FirstOrDefaultAsync(x => x.StableId == record.StableId, cancellationToken);
        if (existing is null)
        {
            record.CreatedAt = DateTime.UtcNow;
            record.UpdatedAt = DateTime.UtcNow;
            _context.KnowledgeRecords.Add(record);
        }
        else
        {
            existing.StatementValue = record.StatementValue;
            existing.NormalizedValue = record.NormalizedValue;
            existing.ConfidenceScore = record.ConfidenceScore;
            existing.VerificationLevel = record.VerificationLevel;
            existing.SourceCount = record.SourceCount;
            existing.UserConfirmationCount = record.UserConfirmationCount;
            existing.UserCorrectionCount = record.UserCorrectionCount;
            existing.MarketplaceConfirmationCount = record.MarketplaceConfirmationCount;
            existing.CatalogConfirmationCount = record.CatalogConfirmationCount;
            existing.ImageMatchConfirmationCount = record.ImageMatchConfirmationCount;
            existing.IsActive = record.IsActive;
            existing.IsDeprecated = record.IsDeprecated;
            existing.ReplacedByKnowledgeRecordId = record.ReplacedByKnowledgeRecordId;
            existing.LastObservedAt = record.LastObservedAt;
            existing.LastVerifiedAt = record.LastVerifiedAt;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.ModelVersion = record.ModelVersion;
            existing.RuleVersion = record.RuleVersion;
            record = existing;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return record;
    }
}

public sealed class KnowledgeQueryService : IKnowledgeQueryService
{
    private readonly ApplicationDbContext _context;

    public KnowledgeQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<KnowledgeRecord?> GetByStableIdAsync(string stableId, CancellationToken cancellationToken = default)
    {
        return _context.KnowledgeRecords
            .AsNoTracking()
            .Include(x => x.Evidence)
            .FirstOrDefaultAsync(x => x.StableId == stableId, cancellationToken);
    }

    public async Task<IReadOnlyList<KnowledgeRecord>> GetBySubjectAsync(KnowledgeSubjectType subjectType, string subjectStableId, CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeRecords
            .AsNoTracking()
            .Where(x => x.SubjectType == subjectType && x.SubjectStableId == subjectStableId)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);
    }
}

public sealed class KnowledgeEvidenceService : IKnowledgeEvidenceService
{
    private readonly ApplicationDbContext _context;

    public KnowledgeEvidenceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<KnowledgeEvidence> AddEvidenceAsync(KnowledgeEvidence evidence, CancellationToken cancellationToken = default)
    {
        evidence.CreatedAt = DateTime.UtcNow;
        _context.KnowledgeEvidence.Add(evidence);

        var record = await _context.KnowledgeRecords.FirstOrDefaultAsync(x => x.Id == evidence.KnowledgeRecordId, cancellationToken);
        if (record is not null)
        {
            record.SourceCount += 1;
            record.LastObservedAt = DateTime.UtcNow;
            record.UpdatedAt = DateTime.UtcNow;
            record.ConfidenceScore = decimal.Clamp(record.ConfidenceScore + evidence.ConfidenceContribution, 0m, 100m);
            if (evidence.IsContradicting)
            {
                record.VerificationLevel = KnowledgeVerificationLevel.Disputed;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return evidence;
    }

    public async Task<IReadOnlyList<KnowledgeEvidence>> GetEvidenceForRecordAsync(Guid knowledgeRecordId, CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeEvidence
            .AsNoTracking()
            .Where(x => x.KnowledgeRecordId == knowledgeRecordId)
            .OrderByDescending(x => x.ObservedAt)
            .ToListAsync(cancellationToken);
    }
}

public sealed class KnowledgeCorrectionService : IKnowledgeCorrectionService
{
    private readonly ApplicationDbContext _context;

    public KnowledgeCorrectionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserCorrection> CreateCorrectionAsync(UserCorrection correction, CancellationToken cancellationToken = default)
    {
        correction.CreatedAt = DateTime.UtcNow;
        if (correction.LearningStatus == 0)
        {
            correction.LearningStatus = LearningStatus.PendingReview;
        }

        _context.UserCorrections.Add(correction);

        var subjectRecords = await _context.KnowledgeRecords
            .Where(x => x.SubjectType == correction.SubjectType && x.SubjectStableId == correction.SubjectStableId)
            .ToListAsync(cancellationToken);

        foreach (var record in subjectRecords)
        {
            record.UserCorrectionCount += 1;
            record.ConfidenceScore = Math.Max(0m, record.ConfidenceScore - 8m);
            record.UpdatedAt = DateTime.UtcNow;
            if (record.ConfidenceScore < 40m)
            {
                record.VerificationLevel = KnowledgeVerificationLevel.Disputed;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return correction;
    }

    public async Task<UserCorrection> UpdateLearningStatusAsync(Guid correctionId, LearningStatus status, string? note, CancellationToken cancellationToken = default)
    {
        var correction = await _context.UserCorrections.FirstAsync(x => x.Id == correctionId, cancellationToken);
        correction.LearningStatus = status;
        correction.ReviewedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(note))
        {
            correction.UserNotes = string.IsNullOrWhiteSpace(correction.UserNotes)
                ? note
                : $"{correction.UserNotes}\n{note}";
        }

        await _context.SaveChangesAsync(cancellationToken);
        return correction;
    }
}

public sealed class KnowledgeLearningService : IKnowledgeLearningService
{
    public KnowledgeLearningDecision EvaluateAutoLearningAction(CorrectionType correctionType, int repeatedCorrectionCount, bool highImpactAction)
    {
        if (highImpactAction)
        {
            return new KnowledgeLearningDecision
            {
                ApplyAutomatically = false,
                QueueForReview = true,
                ReviewItemType = KnowledgeReviewItemType.HighImpactLearningAction,
                Reason = "High-impact actions require review and cannot be auto-applied."
            };
        }

        if (repeatedCorrectionCount >= 3)
        {
            return new KnowledgeLearningDecision
            {
                ApplyAutomatically = true,
                QueueForReview = false,
                Reason = "Repeated confirmed local signal can adjust local ranking/aliases."
            };
        }

        return new KnowledgeLearningDecision
        {
            ApplyAutomatically = false,
            QueueForReview = true,
            ReviewItemType = correctionType == CorrectionType.Ocr
                ? KnowledgeReviewItemType.RepeatedOcrCorrection
                : KnowledgeReviewItemType.RepeatedUserCorrection,
            Reason = "Insufficient repetition for automatic learning; queued for review."
        };
    }
}

public sealed class DecisionHistoryService : IDecisionHistoryService
{
    private readonly ApplicationDbContext _context;

    public DecisionHistoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DecisionHistoryRecord> RecordDecisionAsync(DecisionHistoryRecord record, CancellationToken cancellationToken = default)
    {
        record.CreatedAt = DateTime.UtcNow;
        _context.DecisionHistoryRecords.Add(record);
        await _context.SaveChangesAsync(cancellationToken);
        return record;
    }

    public string BuildExplanationSummary(DecisionExplanationInput input)
    {
        var support = input.StrongestSupportingFactors.Any()
            ? string.Join(", ", input.StrongestSupportingFactors.Take(3))
            : "none";
        var contradictions = input.ImportantContradictions.Any()
            ? string.Join(", ", input.ImportantContradictions.Take(2))
            : "none";
        var missing = input.MissingInformation.Any()
            ? string.Join(", ", input.MissingInformation.Take(2))
            : "none";
        var alternatives = input.AlternativesConsidered.Any()
            ? string.Join(", ", input.AlternativesConsidered.Take(3))
            : "none";

        return $"Selected: {input.SelectedOption ?? "Unknown"}; Confidence: {Math.Round(input.ConfidenceScore, 2)}; Supporting: {support}; Contradictions: {contradictions}; Missing: {missing}; Alternatives: {alternatives}; User Action: {input.UserAction ?? "none"}.";
    }
}

public sealed class KnowledgeHealthService : IKnowledgeHealthService
{
    private readonly ApplicationDbContext _context;

    public KnowledgeHealthService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<KnowledgeHealthReport> RunHealthChecksAsync(CancellationToken cancellationToken = default)
    {
        var issues = new List<KnowledgeHealthIssue>();

        var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
        if (!canConnect)
        {
            return new KnowledgeHealthReport
            {
                OverallState = KnowledgeHealthState.Failed,
                Issues = new[]
                {
                    new KnowledgeHealthIssue
                    {
                        CheckCode = "knowledge-db-connection",
                        State = KnowledgeHealthState.Failed,
                        Summary = "Knowledge database is unavailable."
                    }
                }
            };
        }

        var orphanEvidence = await _context.KnowledgeEvidence
            .AsNoTracking()
            .CountAsync(x => !_context.KnowledgeRecords.Any(record => record.Id == x.KnowledgeRecordId), cancellationToken);
        if (orphanEvidence > 0)
        {
            issues.Add(new KnowledgeHealthIssue
            {
                CheckCode = "knowledge-orphan-evidence",
                State = KnowledgeHealthState.Warning,
                Summary = $"Found {orphanEvidence} evidence record(s) without knowledge subjects."
            });
        }

        var recordsWithoutEvidence = await _context.KnowledgeRecords
            .AsNoTracking()
            .CountAsync(record => !_context.KnowledgeEvidence.Any(evidence => evidence.KnowledgeRecordId == record.Id), cancellationToken);
        if (recordsWithoutEvidence > 0)
        {
            issues.Add(new KnowledgeHealthIssue
            {
                CheckCode = "knowledge-records-without-evidence",
                State = KnowledgeHealthState.Warning,
                Summary = $"Found {recordsWithoutEvidence} knowledge record(s) without evidence."
            });
        }

        var duplicateStableIds = await _context.KnowledgeRecords
            .AsNoTracking()
            .GroupBy(x => x.StableId)
            .CountAsync(group => group.Count() > 1, cancellationToken);
        if (duplicateStableIds > 0)
        {
            issues.Add(new KnowledgeHealthIssue
            {
                CheckCode = "knowledge-duplicate-stable-id",
                State = KnowledgeHealthState.Failed,
                Summary = $"Found {duplicateStableIds} duplicate stable ID group(s)."
            });
        }

        var invalidConfidence = await _context.KnowledgeRecords
            .AsNoTracking()
            .CountAsync(x => x.ConfidenceScore < 0m || x.ConfidenceScore > 100m, cancellationToken);
        if (invalidConfidence > 0)
        {
            issues.Add(new KnowledgeHealthIssue
            {
                CheckCode = "knowledge-invalid-confidence",
                State = KnowledgeHealthState.Failed,
                Summary = $"Found {invalidConfidence} record(s) with out-of-range confidence values."
            });
        }

        var missingModelVersions = await _context.KnowledgeRecords
            .AsNoTracking()
            .CountAsync(x => x.ModelVersion == null, cancellationToken);
        if (missingModelVersions > 0)
        {
            issues.Add(new KnowledgeHealthIssue
            {
                CheckCode = "knowledge-missing-model-version",
                State = KnowledgeHealthState.Warning,
                Summary = $"Found {missingModelVersions} record(s) without model version."
            });
        }

        var missingRuleVersions = await _context.KnowledgeRecords
            .AsNoTracking()
            .CountAsync(x => x.RuleVersion == null, cancellationToken);
        if (missingRuleVersions > 0)
        {
            issues.Add(new KnowledgeHealthIssue
            {
                CheckCode = "knowledge-missing-rule-version",
                State = KnowledgeHealthState.Warning,
                Summary = $"Found {missingRuleVersions} record(s) without rule version."
            });
        }

        var pendingCorrections = await _context.UserCorrections
            .AsNoTracking()
            .CountAsync(x => x.LearningStatus == LearningStatus.PendingReview, cancellationToken);
        if (pendingCorrections > 0)
        {
            issues.Add(new KnowledgeHealthIssue
            {
                CheckCode = "knowledge-pending-corrections",
                State = KnowledgeHealthState.Warning,
                Summary = $"Found {pendingCorrections} pending correction(s)."
            });
        }

        var stuckReviewItems = await _context.KnowledgeReviewItems
            .AsNoTracking()
            .CountAsync(x => x.Status == KnowledgeReviewQueueState.InReview && x.CreatedAt < DateTime.UtcNow.AddDays(-7), cancellationToken);
        if (stuckReviewItems > 0)
        {
            issues.Add(new KnowledgeHealthIssue
            {
                CheckCode = "knowledge-stuck-review-items",
                State = KnowledgeHealthState.Warning,
                Summary = $"Found {stuckReviewItems} stuck review item(s)."
            });
        }

        var overall = issues.Any(x => x.State == KnowledgeHealthState.Failed)
            ? KnowledgeHealthState.Failed
            : issues.Any()
                ? KnowledgeHealthState.Warning
                : KnowledgeHealthState.Healthy;

        return new KnowledgeHealthReport
        {
            OverallState = overall,
            Issues = issues
        };
    }
}

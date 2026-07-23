using IBSCardManager.Entities;
using IBSCardManager.Models;

namespace IBSCardManager.Services;

public interface IKnowledgeService
{
    Task<KnowledgeRecord> UpsertKnowledgeRecordAsync(KnowledgeRecord record, CancellationToken cancellationToken = default);
}

public interface IKnowledgeQueryService
{
    Task<KnowledgeRecord?> GetByStableIdAsync(string stableId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KnowledgeRecord>> GetBySubjectAsync(KnowledgeSubjectType subjectType, string subjectStableId, CancellationToken cancellationToken = default);
}

public interface IKnowledgeLearningService
{
    KnowledgeLearningDecision EvaluateAutoLearningAction(CorrectionType correctionType, int repeatedCorrectionCount, bool highImpactAction);
}

public interface IKnowledgeEvidenceService
{
    Task<KnowledgeEvidence> AddEvidenceAsync(KnowledgeEvidence evidence, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KnowledgeEvidence>> GetEvidenceForRecordAsync(Guid knowledgeRecordId, CancellationToken cancellationToken = default);
}

public interface IKnowledgeCorrectionService
{
    Task<UserCorrection> CreateCorrectionAsync(UserCorrection correction, CancellationToken cancellationToken = default);
    Task<UserCorrection> UpdateLearningStatusAsync(Guid correctionId, LearningStatus status, string? note, CancellationToken cancellationToken = default);
}

public interface IConfidenceScoringService
{
    ConfidenceScoreResult Calculate(ConfidenceScoringInput input);
}

public interface IDecisionHistoryService
{
    Task<DecisionHistoryRecord> RecordDecisionAsync(DecisionHistoryRecord record, CancellationToken cancellationToken = default);
    string BuildExplanationSummary(DecisionExplanationInput input);
}

public interface IKnowledgeModelVersionService
{
    KnowledgeVersionInfo GetCurrentVersions();
}

public interface IKnowledgeHealthService
{
    Task<KnowledgeHealthReport> RunHealthChecksAsync(CancellationToken cancellationToken = default);
}

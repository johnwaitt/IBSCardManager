using IBSCardManager.Models;

namespace IBSCardManager.Services;

public interface ICollectionInsightsService
{
    Task<CollectionAnalyticsDashboardViewModel> BuildCollectionAnalyticsDashboardAsync(string range, CancellationToken cancellationToken = default);
    Task<RecommendationCenterViewModel> BuildRecommendationCenterAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AnalyticsCardScoreViewModel>> GetTopReportAsync(string reportCode, int take = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CollectionConcentrationRowViewModel>> GetConcentrationAsync(string dimension, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DuplicateAnalyticsRowViewModel>> GetDuplicateAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DataQualityIssueViewModel>> GetDataQualityIssuesAsync(CancellationToken cancellationToken = default);
    Task<SnapshotCreationResult> CreateSnapshotAsync(string trigger, CancellationToken cancellationToken = default);
    Task RecalculateAnalyticsAsync(string trigger, CancellationToken cancellationToken = default);
}

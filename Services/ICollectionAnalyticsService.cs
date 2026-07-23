using IBSCardManager.Models;

namespace IBSCardManager.Services;

public interface ICollectionAnalyticsService
{
    Task<DashboardViewModel> BuildDashboardAsync(CancellationToken cancellationToken = default);
    Task<CollectionExplorerViewModel> BuildExplorerAsync(string mode, string? search, int page, int pageSize, string? sort, CancellationToken cancellationToken = default);
    Task<CollectionExplorerDetailViewModel> BuildDetailAsync(string mode, string id, CancellationToken cancellationToken = default);
}

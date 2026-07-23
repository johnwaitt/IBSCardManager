using IBSCardManager.Entities;

namespace IBSCardManager.Services;

public interface ICatalogSearchService
{
    Task<IReadOnlyList<Product>> SearchProductsAsync(string? query, int take = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChecklistItem>> SearchChecklistCardsAsync(string? query, int take = 100, CancellationToken cancellationToken = default);
}

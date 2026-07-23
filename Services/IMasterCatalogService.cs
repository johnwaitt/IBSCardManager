using IBSCardManager.Entities;

namespace IBSCardManager.Services;

public interface IMasterCatalogService
{
    Task<Product?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ChecklistItem?> GetChecklistItemAsync(Guid checklistItemId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> SearchProductsAsync(string? search, int take = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChecklistItem>> SearchChecklistItemsAsync(Guid? productId, string? search, int take = 100, CancellationToken cancellationToken = default);
}

using IBSCardManager.Entities;

namespace IBSCardManager.Services;

public interface ICatalogLookupService
{
    Task<Product?> FindProductByStableIdAsync(string catalogRecordId, CancellationToken cancellationToken = default);
    Task<ChecklistItem?> FindChecklistCardByStableIdAsync(string catalogRecordId, CancellationToken cancellationToken = default);
}

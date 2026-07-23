namespace IBSCardManager.Services;

public interface ICatalogVersionService
{
    Task<string?> GetCatalogVersionAsync(CancellationToken cancellationToken = default);
    Task<DateTime?> GetCatalogUpdatedAtAsync(CancellationToken cancellationToken = default);
}

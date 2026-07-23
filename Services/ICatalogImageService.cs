namespace IBSCardManager.Services;

public interface ICatalogImageService
{
    Task<string?> GetFrontImageUrlAsync(Guid checklistItemId, CancellationToken cancellationToken = default);
    Task<string?> GetBackImageUrlAsync(Guid checklistItemId, CancellationToken cancellationToken = default);
}

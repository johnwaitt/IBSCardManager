namespace IBSCardManager.Services;

public interface ICatalogDatabaseProvider
{
    string ProviderName { get; }
    string DatabaseRole { get; }
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
}

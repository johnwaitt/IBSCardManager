using IBSCardManager.Models;

namespace IBSCardManager.Services;

public interface IProviderBase
{
    string Name { get; }
    string ProviderType { get; }
    string Version { get; }
    ProviderReadiness Readiness { get; }
    bool IsEnabled { get; }
    Task<ProviderStatusRecord> GetStatusAsync(CancellationToken cancellationToken = default);
    Task<ProviderConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default);
}

public interface IAIProvider : IProviderBase
{
    bool SupportsVision { get; }
    bool SupportsOcr { get; }
    bool SupportsPricing { get; }
    bool SupportsLearning { get; }
}

public interface IMarketplaceProvider : IProviderBase
{
    bool SupportsInventoryExport { get; }
    bool SupportsListingSync { get; }
    bool SupportsSalesSync { get; }
}

public interface IPriceProvider : IProviderBase { }
public interface IStorageProvider : IProviderBase { }
public interface ICloudBackupProvider : IProviderBase { }
public interface IOcrProvider : IProviderBase { }
public interface IAuthenticationProvider : IProviderBase { }

public interface IProviderManager
{
    Task<IReadOnlyList<ProviderStatusRecord>> GetAllProvidersAsync(CancellationToken cancellationToken = default);
    Task<ProviderConnectionTestResult> TestConnectionAsync(string providerName, CancellationToken cancellationToken = default);
    Task<bool> EnableProviderAsync(string providerName, CancellationToken cancellationToken = default);
    Task<bool> DisableProviderAsync(string providerName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProviderHealthResult>> RunHealthChecksAsync(CancellationToken cancellationToken = default);
    Task<ProviderStatusRecord?> GetProviderAsync(string providerName, CancellationToken cancellationToken = default);
}

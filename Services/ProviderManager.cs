using IBSCardManager.Models;

namespace IBSCardManager.Services;

public sealed class ProviderManager : IProviderManager
{
    private readonly IReadOnlyList<IProviderBase> _providers;

    public ProviderManager(IEnumerable<IProviderBase> providers)
    {
        _providers = providers.ToList();
    }

    public async Task<IReadOnlyList<ProviderStatusRecord>> GetAllProvidersAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<ProviderStatusRecord>();
        foreach (var provider in _providers.OrderBy(x => x.ProviderType).ThenBy(x => x.Name))
        {
            list.Add(await provider.GetStatusAsync(cancellationToken));
        }

        return list;
    }

    public async Task<ProviderConnectionTestResult> TestConnectionAsync(string providerName, CancellationToken cancellationToken = default)
    {
        var provider = _providers.FirstOrDefault(x => x.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));
        if (provider is null)
        {
            return new ProviderConnectionTestResult
            {
                ProviderName = providerName,
                Success = false,
                Message = "Provider not found.",
                TestedAt = DateTimeOffset.UtcNow
            };
        }

        return await provider.TestConnectionAsync(cancellationToken);
    }

    public Task<bool> EnableProviderAsync(string providerName, CancellationToken cancellationToken = default)
    {
        var exists = _providers.Any(x => x.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }

    public Task<bool> DisableProviderAsync(string providerName, CancellationToken cancellationToken = default)
    {
        var exists = _providers.Any(x => x.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }

    public async Task<IReadOnlyList<ProviderHealthResult>> RunHealthChecksAsync(CancellationToken cancellationToken = default)
    {
        var output = new List<ProviderHealthResult>();
        foreach (var provider in _providers)
        {
            var test = await provider.TestConnectionAsync(cancellationToken);
            output.Add(new ProviderHealthResult
            {
                ProviderName = provider.Name,
                Healthy = test.Success,
                Message = test.Message
            });
        }

        return output;
    }

    public async Task<ProviderStatusRecord?> GetProviderAsync(string providerName, CancellationToken cancellationToken = default)
    {
        var provider = _providers.FirstOrDefault(x => x.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));
        return provider is null ? null : await provider.GetStatusAsync(cancellationToken);
    }
}

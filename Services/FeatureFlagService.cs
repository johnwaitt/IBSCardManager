using IBSCardManager.Options;
using Microsoft.Extensions.Options;

namespace IBSCardManager.Services;

public interface IFeatureFlagService
{
    bool IsEnabled(string flagName);
    IReadOnlyDictionary<string, bool> GetAll();
    bool IsDeveloperModeEnabled();
}

public sealed class FeatureFlagService : IFeatureFlagService
{
    private readonly FeatureManagementOptions _options;

    public FeatureFlagService(IOptions<FeatureManagementOptions> options)
    {
        _options = options.Value;
    }

    public bool IsEnabled(string flagName)
    {
        if (string.IsNullOrWhiteSpace(flagName)) return false;
        return _options.FeatureFlags.TryGetValue(flagName, out var enabled) && enabled;
    }

    public IReadOnlyDictionary<string, bool> GetAll() => _options.FeatureFlags;

    public bool IsDeveloperModeEnabled() => _options.DeveloperModeEnabled;
}

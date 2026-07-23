using IBSCardManager.Models;
using IBSCardManager.Options;
using Microsoft.Extensions.Options;

namespace IBSCardManager.Services;

public sealed class OpenAiProvider : IAIProvider
{
    private readonly OpenAiProviderOptions _options;
    private readonly OpenAiCardAnalysisOptions _legacyOptions;
    private readonly IOpenAiCardAnalysisService _openAiCardAnalysisService;

    public OpenAiProvider(
        IOptions<OpenAiProviderOptions> options,
        IOptions<OpenAiCardAnalysisOptions> legacyOptions,
        IOpenAiCardAnalysisService openAiCardAnalysisService)
    {
        _options = options.Value;
        _legacyOptions = legacyOptions.Value;
        _openAiCardAnalysisService = openAiCardAnalysisService;
    }

    public string Name => "OpenAI";
    public string ProviderType => "AI";
    public string Version => "v1";
    public ProviderReadiness Readiness => _options.Enabled ? ProviderReadiness.ProductionReady : ProviderReadiness.Disabled;
    public bool IsEnabled => _options.Enabled;
    public bool SupportsVision => _options.EnableVision;
    public bool SupportsOcr => _options.EnableOcr;
    public bool SupportsPricing => _options.EnableAiPricing;
    public bool SupportsLearning => _options.EnableAiLearning;

    public async Task<ProviderStatusRecord> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var test = await TestConnectionAsync(cancellationToken);
        return new ProviderStatusRecord
        {
            ProviderName = Name,
            ProviderType = ProviderType,
            Enabled = IsEnabled,
            Status = test.Success ? "Connected" : "Unavailable",
            Version = Version,
            Readiness = Readiness,
            Capabilities = new[]
            {
                new ProviderCapability { Name = "Vision", Supported = SupportsVision, Availability = SupportsVision ? "Available" : "Unavailable" },
                new ProviderCapability { Name = "OCR", Supported = SupportsOcr, Availability = SupportsOcr ? "Available" : "Unavailable" },
                new ProviderCapability { Name = "AI Pricing", Supported = SupportsPricing, Availability = SupportsPricing ? "Available" : "Unavailable" },
                new ProviderCapability { Name = "Card Identification", Supported = _options.EnableCardIdentification, Availability = _options.EnableCardIdentification ? "Available" : "Unavailable" },
                new ProviderCapability { Name = "AI Learning", Supported = SupportsLearning, Availability = SupportsLearning ? "Available" : "Unavailable" }
            },
            LastTest = test.TestedAt,
            LastSuccess = test.Success ? test.TestedAt : _options.LastSuccessfulRequestUtc,
            LastFailure = test.Success ? null : test.TestedAt,
            ConfigurationSummary = $"Model={(_options.VisionModel ?? _legacyOptions.VisionModel)} Timeout={_options.TimeoutSeconds}s Retries={_options.RetryCount}",
            LogSummary = _options.EnableLogging ? "Logging enabled" : "Logging disabled"
        };
    }

    public async Task<ProviderConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var ok = _options.Enabled && await _openAiCardAnalysisService.TestConnectionAsync(cancellationToken);
        return new ProviderConnectionTestResult
        {
            ProviderName = Name,
            Success = ok,
            Message = ok ? "Connection succeeded." : "Connection failed or provider disabled.",
            TestedAt = DateTimeOffset.UtcNow
        };
    }
}

public sealed class EBayMarketplaceProvider : IMarketplaceProvider
{
    private readonly MarketplaceProviderOptions _options;

    public EBayMarketplaceProvider(IOptions<MarketplaceProviderOptions> options)
    {
        _options = options.Value;
    }

    public string Name => "eBay";
    public string ProviderType => "Marketplace";
    public string Version => "v1";
    public ProviderReadiness Readiness => _options.EBay.Enabled ? ProviderReadiness.ProductionReady : ProviderReadiness.Disabled;
    public bool IsEnabled => _options.EBay.Enabled;
    public bool SupportsInventoryExport => true;
    public bool SupportsListingSync => true;
    public bool SupportsSalesSync => true;

    public Task<ProviderStatusRecord> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var hasCreds = !string.IsNullOrWhiteSpace(_options.EBay.ClientIdEncrypted)
            && !string.IsNullOrWhiteSpace(_options.EBay.RefreshTokenEncrypted);

        var status = new ProviderStatusRecord
        {
            ProviderName = Name,
            ProviderType = ProviderType,
            Enabled = IsEnabled,
            Status = IsEnabled && hasCreds ? "Configured" : "Not Configured",
            Version = Version,
            Readiness = Readiness,
            Capabilities = new[]
            {
                new ProviderCapability { Name = "OAuth", Supported = true, Availability = "Available" },
                new ProviderCapability { Name = "Inventory Sync", Supported = _options.EBay.InventorySync, Availability = _options.EBay.InventorySync ? "Available" : "Disabled" },
                new ProviderCapability { Name = "Order Sync", Supported = _options.EBay.OrderSync, Availability = _options.EBay.OrderSync ? "Available" : "Disabled" },
                new ProviderCapability { Name = "Business Policies", Supported = true, Availability = "Available" }
            },
            LastTest = _options.EBay.LastSyncUtc,
            LastSuccess = _options.EBay.LastSyncUtc,
            LastFailure = null,
            ConfigurationSummary = _options.EBay.Sandbox ? "Sandbox" : "Production",
            LogSummary = "No recent errors"
        };

        return Task.FromResult(status);
    }

    public Task<ProviderConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var configured = !string.IsNullOrWhiteSpace(_options.EBay.ClientIdEncrypted)
            && !string.IsNullOrWhiteSpace(_options.EBay.RefreshTokenEncrypted);

        return Task.FromResult(new ProviderConnectionTestResult
        {
            ProviderName = Name,
            Success = configured,
            Message = configured ? "eBay provider settings are configured." : "Missing encrypted OAuth configuration.",
            TestedAt = DateTimeOffset.UtcNow
        });
    }
}

public sealed class WhatnotMarketplaceProvider : IMarketplaceProvider
{
    private readonly MarketplaceProviderOptions _options;

    public WhatnotMarketplaceProvider(IOptions<MarketplaceProviderOptions> options)
    {
        _options = options.Value;
    }

    public string Name => "Whatnot";
    public string ProviderType => "Marketplace";
    public string Version => "v1";
    public ProviderReadiness Readiness => IsEnabled ? ProviderReadiness.Limited : ProviderReadiness.Disabled;
    public bool IsEnabled => _options.Whatnot.Enabled;
    public bool SupportsInventoryExport => true;
    public bool SupportsListingSync => false;
    public bool SupportsSalesSync => false;

    public Task<ProviderStatusRecord> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var status = new ProviderStatusRecord
        {
            ProviderName = Name,
            ProviderType = ProviderType,
            Enabled = IsEnabled,
            Status = IsEnabled ? "Limited" : "Disabled",
            Version = Version,
            Readiness = Readiness,
            Capabilities = new[]
            {
                new ProviderCapability { Name = "Inventory Export", Supported = true, Availability = "Available" },
                new ProviderCapability { Name = "CSV Export", Supported = true, Availability = "Available" },
                new ProviderCapability { Name = "Inventory Preparation", Supported = true, Availability = "Available" },
                new ProviderCapability { Name = "Connection API", Supported = false, Availability = "Unavailable" },
                new ProviderCapability { Name = "Listing Sync", Supported = false, Availability = "Unavailable" },
                new ProviderCapability { Name = "Sales Sync", Supported = false, Availability = "Unavailable" },
                new ProviderCapability { Name = "Live Stream Sync", Supported = false, Availability = "Unavailable" },
                new ProviderCapability { Name = "Order Sync", Supported = false, Availability = "Unavailable" }
            },
            LastTest = null,
            LastSuccess = null,
            LastFailure = null,
            ConfigurationSummary = "Export Only",
            LogSummary = "Limited capability provider"
        };

        return Task.FromResult(status);
    }

    public Task<ProviderConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderConnectionTestResult
        {
            ProviderName = Name,
            Success = true,
            Message = "Whatnot provider is limited to export workflows; API sync is unavailable.",
            TestedAt = DateTimeOffset.UtcNow
        });
    }
}

public sealed class PlaceholderProvider : IProviderBase
{
    private readonly string _name;
    private readonly string _type;

    public PlaceholderProvider(string name, string type)
    {
        _name = name;
        _type = type;
    }

    public string Name => _name;
    public string ProviderType => _type;
    public string Version => "placeholder";
    public ProviderReadiness Readiness => ProviderReadiness.ComingSoon;
    public bool IsEnabled => false;

    public Task<ProviderStatusRecord> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderStatusRecord
        {
            ProviderName = Name,
            ProviderType = ProviderType,
            Enabled = false,
            Status = "Coming Soon",
            Version = Version,
            Readiness = ProviderReadiness.ComingSoon,
            Capabilities = Array.Empty<ProviderCapability>(),
            ConfigurationSummary = "Placeholder provider",
            LogSummary = "No logs"
        });
    }

    public Task<ProviderConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderConnectionTestResult
        {
            ProviderName = Name,
            Success = false,
            Message = "Provider not implemented yet.",
            TestedAt = DateTimeOffset.UtcNow
        });
    }
}

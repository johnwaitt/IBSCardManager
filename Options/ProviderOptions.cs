namespace IBSCardManager.Options;

public sealed class OpenAiProviderOptions
{
    public bool Enabled { get; set; } = true;
    public string ApiKeyEncrypted { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = "gpt-4.1-mini";
    public string VisionModel { get; set; } = "gpt-4.1-mini";
    public decimal Temperature { get; set; } = 0.2m;
    public int MaximumTokens { get; set; } = 1000;
    public int TimeoutSeconds { get; set; } = 60;
    public int RetryCount { get; set; } = 2;
    public bool EnableVision { get; set; } = true;
    public bool EnableOcr { get; set; } = true;
    public bool EnableAiPricing { get; set; } = true;
    public bool EnableCardIdentification { get; set; } = true;
    public bool EnableAiLearning { get; set; } = true;
    public bool EnableLogging { get; set; } = true;
    public bool EnableUsageStatistics { get; set; } = true;
    public decimal EstimatedCost { get; set; }
    public DateTimeOffset? LastSuccessfulRequestUtc { get; set; }
}

public sealed class MarketplaceProviderOptions
{
    public EBayProviderOptions EBay { get; set; } = new();
    public WhatnotProviderOptions Whatnot { get; set; } = new();
}

public sealed class EBayProviderOptions
{
    public bool Enabled { get; set; }
    public bool Sandbox { get; set; } = true;
    public string ClientIdEncrypted { get; set; } = string.Empty;
    public string ClientSecretEncrypted { get; set; } = string.Empty;
    public string RefreshTokenEncrypted { get; set; } = string.Empty;
    public bool InventorySync { get; set; } = true;
    public bool OrderSync { get; set; }
    public string ShippingPolicyName { get; set; } = string.Empty;
    public string ReturnPolicyName { get; set; } = string.Empty;
    public string PaymentPolicyName { get; set; } = string.Empty;
    public string ListingTemplateName { get; set; } = string.Empty;
    public DateTimeOffset? LastSyncUtc { get; set; }
}

public sealed class WhatnotProviderOptions
{
    public bool Enabled { get; set; }
    public string ExportMode { get; set; } = "ExportOnly";
}

public sealed class FeatureManagementOptions
{
    public bool DeveloperModeEnabled { get; set; }
    public Dictionary<string, bool> FeatureFlags { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

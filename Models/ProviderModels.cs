namespace IBSCardManager.Models;

public enum ProviderReadiness
{
    ProductionReady = 1,
    Limited = 2,
    Beta = 3,
    ComingSoon = 4,
    Disabled = 5,
    Unavailable = 6
}

public sealed class ProviderCapability
{
    public string Name { get; init; } = string.Empty;
    public bool Supported { get; init; }
    public string Availability { get; init; } = "Unavailable";
}

public sealed class ProviderStatusRecord
{
    public string ProviderName { get; init; } = string.Empty;
    public string ProviderType { get; init; } = string.Empty;
    public bool Enabled { get; init; }
    public string Status { get; init; } = "Unknown";
    public string Version { get; init; } = "Unknown";
    public ProviderReadiness Readiness { get; init; } = ProviderReadiness.Unavailable;
    public IReadOnlyList<ProviderCapability> Capabilities { get; init; } = Array.Empty<ProviderCapability>();
    public DateTimeOffset? LastTest { get; init; }
    public DateTimeOffset? LastSuccess { get; init; }
    public DateTimeOffset? LastFailure { get; init; }
    public string ConfigurationSummary { get; init; } = "Not configured";
    public string LogSummary { get; init; } = "No logs";
}

public sealed class ProviderHealthResult
{
    public string ProviderName { get; init; } = string.Empty;
    public bool Healthy { get; init; }
    public string Message { get; init; } = string.Empty;
}

public sealed class ProviderConnectionTestResult
{
    public string ProviderName { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTimeOffset TestedAt { get; init; }
}

public sealed class ProviderDashboardViewModel
{
    public IReadOnlyList<ProviderStatusRecord> Providers { get; init; } = Array.Empty<ProviderStatusRecord>();
    public DateTimeOffset GeneratedAt { get; init; }
}

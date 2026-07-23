namespace IBSCardManager.Models;

public sealed class AdministrationDashboardViewModel
{
    public string ApplicationVersion { get; init; } = string.Empty;
    public string InformationalVersion { get; init; } = string.Empty;
    public IReadOnlyList<AdministrationStatusCardViewModel> Cards { get; init; } = Array.Empty<AdministrationStatusCardViewModel>();
    public DateTimeOffset GeneratedAt { get; init; }
}

public sealed class AdministrationStatusCardViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = "Unknown";
    public string Detail { get; init; } = string.Empty;
    public string? LastEventLabel { get; init; }
    public string? LastEventValue { get; init; }
    public string Severity { get; init; } = "Info";
}

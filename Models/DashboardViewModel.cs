using IBSCardManager.Entities;

namespace IBSCardManager.Models;

public sealed class DashboardViewModel
{
    public int TotalCards { get; init; }
    public int TotalRecords { get; init; }
    public decimal CollectionValue { get; init; }
    public decimal TotalCost { get; init; }
    public int GradedCards { get; init; }
    public int ActiveListings { get; init; }
    public int Autographs { get; init; }
    public int Rookies { get; init; }
    public int Relics { get; init; }
    public int Refractors { get; init; }
    public IReadOnlyList<Card> RecentCards { get; init; } = Array.Empty<Card>();

    public IReadOnlyList<DashboardMetricViewModel> Metrics { get; init; } = Array.Empty<DashboardMetricViewModel>();
    public IReadOnlyList<DashboardSectionViewModel> Sections { get; init; } = Array.Empty<DashboardSectionViewModel>();
    public IReadOnlyList<DashboardActionViewModel> QuickActions { get; init; } = Array.Empty<DashboardActionViewModel>();
    public DateTimeOffset LastRefreshed { get; init; } = DateTimeOffset.MinValue;
    public string DatabaseStatus { get; init; } = "Connected";
    public string BackgroundTaskStatus { get; init; } = "Idle";
    public string RuntimeDatabase { get; init; } = "SQL Server";

    public decimal Gain => CollectionValue - TotalCost;

    public decimal ReturnPercent => TotalCost > 0
        ? (Gain / TotalCost) * 100m
        : 0m;
}

public sealed class DashboardMetricViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string? Subtext { get; init; }
    public string? IconClass { get; init; }
    public string? CssClass { get; init; }
    public bool IsUnavailable { get; init; }
}

public sealed class DashboardSectionStateViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string IconClass { get; init; } = string.Empty;
    public string EmptyMessage { get; init; } = "No items found.";
    public string ActionText { get; init; } = "View all";
    public string? ActionUrl { get; init; }
    public bool IsUnavailable { get; init; }
}

public sealed class DashboardStatusViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string IconClass { get; init; } = string.Empty;
    public string? CssClass { get; init; }
}

public sealed class DashboardActionViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string IconClass { get; init; } = string.Empty;
}

public sealed class DashboardSectionViewModel
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ViewAllUrl { get; init; }
    public string EmptyMessage { get; init; } = "No items found.";
    public string? ErrorMessage { get; init; }
    public bool IsUnavailable { get; init; }
    public IReadOnlyList<CollectionItemViewModel> Items { get; init; } = Array.Empty<CollectionItemViewModel>();
}

public sealed class CollectionItemViewModel
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }
    public string? Meta { get; init; }
    public string? Meta2 { get; init; }
    public string? ImageUrl { get; init; }
    public string? LinkUrl { get; init; }
    public string? Badge { get; init; }
    public string? SecondaryBadge { get; init; }
    public string? ValueText { get; init; }
    public string? QuantityText { get; init; }
    public string? StatusText { get; init; }
    public string? StorageText { get; init; }
    public string? ChecklistText { get; init; }
    public string? CardNumber { get; init; }
    public string? Player { get; init; }
    public string? Team { get; init; }
    public string? Year { get; init; }
    public string? Product { get; init; }
    public string? Brand { get; init; }
    public string? Manufacturer { get; init; }
    public string? Parallel { get; init; }
    public string? Variation { get; init; }
    public string? Grade { get; init; }
    public string? Source { get; init; }
    public int? ProgressPercent { get; init; }
    public decimal? NumericValue { get; init; }
    public int? NumericCount { get; init; }
    public DateTimeOffset? Date { get; init; }
    public bool IsUnavailable { get; init; }
}

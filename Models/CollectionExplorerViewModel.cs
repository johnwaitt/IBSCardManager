namespace IBSCardManager.Models;

public sealed class CollectionExplorerViewModel
{
    public string Mode { get; init; } = "sets";
    public string Title { get; init; } = "Collection Explorer";
    public string Description { get; init; } = string.Empty;
    public string Search { get; init; } = string.Empty;
    public string? Sort { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 24;
    public int TotalCount { get; init; }
    public IReadOnlyList<ExplorerModeOptionViewModel> Modes { get; init; } = Array.Empty<ExplorerModeOptionViewModel>();
    public IReadOnlyList<CollectionExplorerFilterViewModel> Filters { get; init; } = Array.Empty<CollectionExplorerFilterViewModel>();
    public IReadOnlyList<CollectionItemViewModel> Items { get; init; } = Array.Empty<CollectionItemViewModel>();
    public IReadOnlyList<DashboardMetricViewModel> SummaryMetrics { get; init; } = Array.Empty<DashboardMetricViewModel>();
    public string EmptyMessage { get; init; } = "No cards found.";
    public string? ErrorMessage { get; init; }
    public bool IsUnavailable { get; init; }

    public int TotalPages => PageSize <= 0
        ? 0
        : (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

public sealed class ExplorerModeOptionViewModel
{
    public string Code { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Url { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public sealed class CollectionExplorerFilterViewModel
{
    public string Name { get; init; } = string.Empty;
    public string? Value { get; init; }
    public string? Url { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CollectionExplorerDetailViewModel
{
    public string Mode { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }
    public string BackUrl { get; init; } = "/CollectionExplorer";
    public IReadOnlyList<DashboardMetricViewModel> SummaryMetrics { get; init; } = Array.Empty<DashboardMetricViewModel>();
    public IReadOnlyList<CollectionItemViewModel> Items { get; init; } = Array.Empty<CollectionItemViewModel>();
    public string EmptyMessage { get; init; } = "No cards found.";
    public bool IsUnavailable { get; init; }
}

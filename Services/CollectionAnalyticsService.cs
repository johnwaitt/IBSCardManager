using System.Globalization;
using System.Linq.Expressions;
using IBSCardManager.Data;
using IBSCardManager.Entities;
using IBSCardManager.Models;
using IBSCardManager.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace IBSCardManager.Services;

public sealed class CollectionAnalyticsService : ICollectionAnalyticsService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly DashboardOptions _options;

    public CollectionAnalyticsService(
        ApplicationDbContext context,
        IMemoryCache cache,
        IOptions<DashboardOptions> options)
    {
        _context = context;
        _cache = cache;
        _options = options.Value;
    }

    public Task<DashboardViewModel> BuildDashboardAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "dashboard:summary:v1";
        return _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Math.Max(1, _options.CacheMinutes));
            return await BuildDashboardCoreAsync(cancellationToken);
        })!;
    }

    public async Task<CollectionExplorerViewModel> BuildExplorerAsync(string mode, string? search, int page, int pageSize, string? sort, CancellationToken cancellationToken = default)
    {
        var normalizedMode = NormalizeMode(mode);
        var normalizedSearch = search?.Trim() ?? string.Empty;
        var normalizedPage = Math.Max(1, page);
        var normalizedPageSize = pageSize is < 1 ? 24 : Math.Min(pageSize, 100);

        var modes = BuildModeOptions(normalizedMode, normalizedSearch, normalizedSort: sort, normalizedPage, normalizedPageSize);
        var filters = BuildFilters(normalizedMode, normalizedSearch, sort, normalizedPage, normalizedPageSize);

        var result = normalizedMode switch
        {
            "players" => await BuildPlayerExplorerAsync(normalizedSearch, normalizedPage, normalizedPageSize, cancellationToken),
            "teams" => await BuildTeamExplorerAsync(normalizedSearch, normalizedPage, normalizedPageSize, cancellationToken),
            "years" => await BuildYearExplorerAsync(normalizedSearch, normalizedPage, normalizedPageSize, cancellationToken),
            "manufacturers" => await BuildManufacturerExplorerAsync(normalizedSearch, normalizedPage, normalizedPageSize, cancellationToken),
            "brands" => await BuildBrandExplorerAsync(normalizedSearch, normalizedPage, normalizedPageSize, cancellationToken),
            "checklistsections" => await BuildChecklistSectionExplorerAsync(normalizedSearch, normalizedPage, normalizedPageSize, cancellationToken),
            "autographs" => await BuildFlagExplorerAsync("Autographs", c => c.IsAutograph, normalizedSearch, normalizedPage, normalizedPageSize, cancellationToken),
            "relics" => await BuildFlagExplorerAsync("Relics", c => c.IsRelic, normalizedSearch, normalizedPage, normalizedPageSize, cancellationToken),
            "parallels" => await BuildParallelExplorerAsync(normalizedSearch, normalizedPage, normalizedPageSize, cancellationToken),
            "gradedcards" => await BuildGradedExplorerAsync(normalizedSearch, normalizedPage, normalizedPageSize, cancellationToken),
            "duplicates" => await BuildDuplicateExplorerAsync(normalizedSearch, normalizedPage, normalizedPageSize, cancellationToken),
            "recentlyadded" => await BuildRecentlyAddedExplorerAsync(normalizedSearch, normalizedPage, normalizedPageSize, cancellationToken),
            "cardsforsale" => await BuildForSaleExplorerAsync(normalizedSearch, normalizedPage, normalizedPageSize, cancellationToken),
            "unmatchedcards" => await BuildUnmatchedExplorerAsync(normalizedSearch, normalizedPage, normalizedPageSize, cancellationToken),
            "wishlist" => BuildUnavailableExplorer("Wishlist", "Wishlist data is not available in the current schema."),
            _ => await BuildSetExplorerAsync(normalizedSearch, normalizedPage, normalizedPageSize, cancellationToken)
        };

        return new CollectionExplorerViewModel
        {
            Mode = normalizedMode,
            Title = result.Title,
            Description = result.Description,
            Search = normalizedSearch,
            Sort = sort,
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            TotalCount = result.TotalCount,
            Modes = modes,
            Filters = filters,
            Items = result.Items,
            SummaryMetrics = result.SummaryMetrics,
            EmptyMessage = result.EmptyMessage,
            ErrorMessage = result.ErrorMessage,
            IsUnavailable = result.IsUnavailable
        };
    }

    public async Task<CollectionExplorerDetailViewModel> BuildDetailAsync(string mode, string id, CancellationToken cancellationToken = default)
    {
        var normalizedMode = NormalizeMode(mode);
        return normalizedMode switch
        {
            "players" => await BuildPlayerDetailAsync(id, cancellationToken),
            "teams" => await BuildTeamDetailAsync(id, cancellationToken),
            "years" => await BuildYearDetailAsync(id, cancellationToken),
            "manufacturers" => await BuildManufacturerDetailAsync(id, cancellationToken),
            "brands" => await BuildBrandDetailAsync(id, cancellationToken),
            _ => await BuildSetDetailAsync(id, cancellationToken)
        };
    }

    private sealed class SetSummary
    {
        public Guid ProductId { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public int Year { get; init; }
        public string BrandName { get; init; } = string.Empty;
        public int ChecklistCount { get; init; }
        public int UniqueOwned { get; init; }
        public int Pieces { get; init; }
    }

    private async Task<DashboardViewModel> BuildDashboardCoreAsync(CancellationToken cancellationToken)
    {
        var cardsQuery = _context.Cards.AsNoTracking().Include(card => card.Product).ThenInclude(product => product!.Brand).AsQueryable();
        var productsQuery = _context.Products.AsNoTracking().Include(product => product.Brand).AsQueryable();

        var totalRecords = await cardsQuery.CountAsync(cancellationToken);
        var totalPieces = await cardsQuery.SumAsync(card => (int?)card.Quantity, cancellationToken) ?? 0;
        var hasAnyValue = await cardsQuery.AnyAsync(card => card.MyValue != null, cancellationToken);
        var collectionValue = await cardsQuery.SumAsync(card => card.MyValue ?? 0m, cancellationToken);
        var totalCost = await cardsQuery.SumAsync(card => card.MyCost ?? 0m, cancellationToken);
        var gradedCards = await cardsQuery.Where(card => !string.IsNullOrWhiteSpace(card.GradeIssuer)).SumAsync(card => card.Quantity, cancellationToken);
        var activeListings = await cardsQuery.Where(card => !string.IsNullOrWhiteSpace(card.ListingStatus) && card.ListingStatus != "Not Listed").SumAsync(card => card.Quantity, cancellationToken);
        var autographs = await cardsQuery.Where(card => card.IsAutograph).SumAsync(card => card.Quantity, cancellationToken);
        var relics = await cardsQuery.Where(card => card.IsRelic).SumAsync(card => card.Quantity, cancellationToken);
        var rookies = await cardsQuery.Where(card => card.IsRookie).SumAsync(card => card.Quantity, cancellationToken);
        var refractors = await cardsQuery.Where(card => card.IsRefractor).SumAsync(card => card.Quantity, cancellationToken);
        var totalSets = await productsQuery.CountAsync(cancellationToken);
        var recentCards = await cardsQuery
            .OrderByDescending(card => card.ModifiedDate)
            .ThenByDescending(card => card.CreatedDate)
            .Take(8)
            .ToListAsync(cancellationToken);
        var setSummaries = await productsQuery
            .Select(product => new SetSummary
            {
                ProductId = product.ProductId,
                DisplayName = product.DisplayName,
                Year = product.Year,
                BrandName = product.Brand!.BrandName,
                ChecklistCount = _context.ChecklistItems.Count(item => item.ProductId == product.ProductId),
                UniqueOwned = _context.Cards.Count(card => card.ProductId == product.ProductId && card.ChecklistItemId != null && card.Quantity > 0),
                Pieces = _context.Cards.Where(card => card.ProductId == product.ProductId).Sum(card => (int?)card.Quantity) ?? 0
            })
            .ToListAsync(cancellationToken);

        var averageCompletion = setSummaries.Any(summary => summary.ChecklistCount > 0)
            ? Math.Round(setSummaries.Where(summary => summary.ChecklistCount > 0).Average(summary => summary.ChecklistCount == 0 ? 0 : (decimal)summary.UniqueOwned / summary.ChecklistCount * 100m), 1)
            : 0m;
        var completedSets = setSummaries.Count(summary => summary.ChecklistCount > 0 && summary.UniqueOwned >= summary.ChecklistCount);
        var unmatchedInventory = await cardsQuery.CountAsync(card => card.ChecklistItemId == null || card.ProductId == null, cancellationToken);
        var cardsForSale = await cardsQuery.CountAsync(card => !string.IsNullOrWhiteSpace(card.ListingStatus) && card.ListingStatus != "Not Listed", cancellationToken);
        var wishlistCards = 0;

        return new DashboardViewModel
        {
            TotalCards = totalPieces,
            TotalRecords = totalRecords,
            CollectionValue = collectionValue,
            TotalCost = totalCost,
            GradedCards = gradedCards,
            ActiveListings = activeListings,
            Autographs = autographs,
            Rookies = rookies,
            Relics = relics,
            Refractors = refractors,
            RecentCards = recentCards,
            LastRefreshed = DateTimeOffset.UtcNow,
            RuntimeDatabase = "SQL Server",
            DatabaseStatus = "Connected",
            BackgroundTaskStatus = "Idle",
            Metrics = BuildDashboardMetrics(totalPieces, totalRecords, collectionValue, totalCost, totalSets, completedSets, averageCompletion, autographs, relics, gradedCards, cardsForSale, wishlistCards, unmatchedInventory, hasAnyValue),
            QuickActions = BuildDashboardActions(),
            Sections = await BuildDashboardSectionsAsync(recentCards, setSummaries, cardsQuery, cancellationToken)
        };
    }

    private static IReadOnlyList<DashboardMetricViewModel> BuildDashboardMetrics(
        int totalPieces,
        int totalRecords,
        decimal collectionValue,
        decimal totalCost,
        int totalSets,
        int completedSets,
        decimal averageCompletion,
        int autographs,
        int relics,
        int gradedCards,
        int cardsForSale,
        int wishlistCards,
        int unmatchedInventory,
        bool hasAnyValue)
    {
        return new[]
        {
            new DashboardMetricViewModel { Title = "Total Collection Value", Value = hasAnyValue ? collectionValue.ToString("C0", CultureInfo.CurrentCulture) : "Value unavailable", Subtext = totalCost > 0 ? $"Gain {(collectionValue - totalCost):C0}" : "Cost data available", IconClass = "bi-cash-coin", CssClass = "accent-green", IsUnavailable = !hasAnyValue },
            new DashboardMetricViewModel { Title = "Total Physical Cards", Value = totalPieces.ToString("N0", CultureInfo.CurrentCulture), Subtext = $"{totalRecords:N0} records", IconClass = "bi-stack", CssClass = "accent-blue" },
            new DashboardMetricViewModel { Title = "Unique Cards Owned", Value = totalRecords.ToString("N0", CultureInfo.CurrentCulture), Subtext = "Distinct inventory entries", IconClass = "bi-card-checklist", CssClass = "accent-purple" },
            new DashboardMetricViewModel { Title = "Total Sets", Value = totalSets.ToString("N0", CultureInfo.CurrentCulture), Subtext = $"{completedSets:N0} completed", IconClass = "bi-layers", CssClass = "accent-orange" },
            new DashboardMetricViewModel { Title = "Average Set Completion", Value = totalSets > 0 ? $"{averageCompletion:N1}%" : "Value unavailable", Subtext = totalSets > 0 ? "Across checked sets" : "No checklist data", IconClass = "bi-graph-up-arrow", CssClass = "accent-teal", IsUnavailable = totalSets == 0 },
            new DashboardMetricViewModel { Title = "Autographs", Value = autographs.ToString("N0", CultureInfo.CurrentCulture), IconClass = "bi-pen-nib", CssClass = "accent-gold" },
            new DashboardMetricViewModel { Title = "Relics", Value = relics.ToString("N0", CultureInfo.CurrentCulture), IconClass = "bi-shield-check", CssClass = "accent-maroon" },
            new DashboardMetricViewModel { Title = "Graded Cards", Value = gradedCards.ToString("N0", CultureInfo.CurrentCulture), IconClass = "bi-award", CssClass = "accent-slate" },
            new DashboardMetricViewModel { Title = "Cards for Sale", Value = cardsForSale.ToString("N0", CultureInfo.CurrentCulture), IconClass = "bi-tag", CssClass = "accent-cyan" },
            new DashboardMetricViewModel { Title = "Wishlist Cards", Value = wishlistCards > 0 ? wishlistCards.ToString("N0", CultureInfo.CurrentCulture) : "Value unavailable", IconClass = "bi-star", CssClass = "accent-pink", IsUnavailable = wishlistCards == 0 },
            new DashboardMetricViewModel { Title = "Unmatched Inventory", Value = unmatchedInventory.ToString("N0", CultureInfo.CurrentCulture), IconClass = "bi-link-45deg", CssClass = "accent-red" }
        };
    }

    private static IReadOnlyList<DashboardActionViewModel> BuildDashboardActions() => new[]
    {
        new DashboardActionViewModel { Title = "Add Card", Description = "Create a new inventory card", Url = "/Cards/Create", IconClass = "bi-plus-circle" },
        new DashboardActionViewModel { Title = "Import Checklist", Description = "Load checklist data", Url = "/Checklists/Import", IconClass = "bi-file-earmark-arrow-up" },
        new DashboardActionViewModel { Title = "Scan Cards", Description = "Open the scanner workflow", Url = "/Scanner", IconClass = "bi-upc-scan" },
        new DashboardActionViewModel { Title = "Open My Collection", Description = "Browse set-based collection", Url = "/Collection", IconClass = "bi-grid-1x2" },
        new DashboardActionViewModel { Title = "Open Collection Explorer", Description = "Browse by players, teams, years, and more", Url = "/CollectionExplorer", IconClass = "bi-compass" },
        new DashboardActionViewModel { Title = "Open Unmatched Cards", Description = "Review unmatched inventory", Url = "/CollectionExplorer?mode=unmatchedcards", IconClass = "bi-link-45deg" },
        new DashboardActionViewModel { Title = "View Wishlist", Description = "Wishlist items and targets", Url = "/CollectionExplorer?mode=wishlist", IconClass = "bi-star" },
        new DashboardActionViewModel { Title = "View Marketplace", Description = "Marketplace drafts and listings", Url = "/Search", IconClass = "bi-shop" },
        new DashboardActionViewModel { Title = "Refresh Dashboard", Description = "Reload the latest aggregates", Url = "/", IconClass = "bi-arrow-clockwise" }
    };

    private async Task<IReadOnlyList<DashboardSectionViewModel>> BuildDashboardSectionsAsync(
        IReadOnlyList<Card> recentCards,
        IReadOnlyList<SetSummary> setSummaries,
        IQueryable<Card> cardsQuery,
        CancellationToken cancellationToken)
    {
        var recentItems = recentCards.Select(card => new CollectionItemViewModel
        {
            Id = card.CardId.ToString(),
            Title = card.Subject,
            Subtitle = card.Product?.DisplayName ?? card.Set,
            Meta = string.IsNullOrWhiteSpace(card.CardNumber) ? null : $"Card {card.CardNumber}",
            Meta2 = string.IsNullOrWhiteSpace(card.Team) ? null : card.Team,
            ImageUrl = card.FrontImagePath,
            LinkUrl = $"/Cards/Details/{card.CardId}",
            Badge = string.IsNullOrWhiteSpace(card.Variety) ? null : card.Variety,
            SecondaryBadge = card.Quantity > 1 ? $"Qty {card.Quantity}" : null,
            QuantityText = $"Qty {card.Quantity}",
            ValueText = card.MyValue.HasValue ? card.MyValue.Value.ToString("C0", CultureInfo.CurrentCulture) : null,
            StorageText = string.Join(" / ", new[] { card.StorageBin, card.StorageBox, card.StorageRow }.Where(part => !string.IsNullOrWhiteSpace(part)))
        }).ToList();

        var mostComplete = setSummaries
            .Select(summary => new CollectionItemViewModel
            {
                Id = summary.ProductId.ToString(),
                Title = summary.DisplayName,
                Subtitle = summary.BrandName,
                Meta = summary.Year.ToString(),
                ProgressPercent = summary.ChecklistCount == 0 ? 0 : (int)Math.Round((decimal)summary.UniqueOwned / summary.ChecklistCount * 100m),
                QuantityText = $"{summary.UniqueOwned:N0} / {summary.ChecklistCount:N0}",
                LinkUrl = $"/Checklists/AddSetToCollection?productId={summary.ProductId}"
            })
            .OrderByDescending(item => item.ProgressPercent)
            .Take(6)
            .ToList();

        var cardsNeedingPricing = await cardsQuery
            .Where(card => card.MyValue == null || (card.ListingStatus != null && card.ListingStatus == "For Sale" && card.ListingPrice == null))
            .OrderByDescending(card => card.ModifiedDate)
            .Take(6)
            .ToListAsync(cancellationToken);

        var unmatched = await cardsQuery
            .Where(card => card.ChecklistItemId == null || card.ProductId == null)
            .OrderByDescending(card => card.ModifiedDate)
            .Take(6)
            .ToListAsync(cancellationToken);

        return new[]
        {
            new DashboardSectionViewModel
            {
                Title = "Recently Added Cards",
                Description = "Latest inventory cards in your collection.",
                ViewAllUrl = "/Cards",
                EmptyMessage = "No cards found.",
                Items = recentItems
            },
            new DashboardSectionViewModel
            {
                Title = "Most Complete Sets",
                Description = "Sets closest to completion.",
                ViewAllUrl = "/Collection",
                Items = mostComplete
            },
            new DashboardSectionViewModel
            {
                Title = "Cards Needing Pricing",
                Description = "Inventory cards without a market value or asking price.",
                ViewAllUrl = "/Cards",
                Items = cardsNeedingPricing.Select(card => new CollectionItemViewModel
                {
                    Id = card.CardId.ToString(),
                    Title = card.Subject,
                    Subtitle = card.Product?.DisplayName ?? card.Set,
                    Meta = card.Year?.ToString(CultureInfo.CurrentCulture),
                    Meta2 = card.CardNumber,
                    ImageUrl = card.FrontImagePath,
                    LinkUrl = $"/Cards/Details/{card.CardId}",
                    QuantityText = $"Qty {card.Quantity}",
                    StatusText = card.ListingPrice.HasValue ? card.ListingPrice.Value.ToString("C0", CultureInfo.CurrentCulture) : "Pricing unavailable"
                }).ToList()
            },
            new DashboardSectionViewModel
            {
                Title = "Unmatched Inventory",
                Description = "Cards not linked to a checklist item.",
                ViewAllUrl = "/CollectionExplorer?mode=unmatchedcards",
                EmptyMessage = "No unmatched inventory found.",
                Items = unmatched.Select(card => new CollectionItemViewModel
                {
                    Id = card.CardId.ToString(),
                    Title = card.Subject,
                    Subtitle = card.Product?.DisplayName ?? card.Set ?? card.StockImageUrl,
                    Meta = card.Year?.ToString(CultureInfo.CurrentCulture),
                    Meta2 = card.CardNumber,
                    ImageUrl = card.FrontImagePath,
                    LinkUrl = $"/Cards/Details/{card.CardId}",
                    StatusText = "Unmatched"
                }).ToList()
            }
        };
    }

    private static CollectionExplorerViewModel BuildUnavailableExplorer(string title, string message) => new()
    {
        Title = title,
        Description = message,
        IsUnavailable = true,
        EmptyMessage = message,
        Items = Array.Empty<CollectionItemViewModel>()
    };

    private async Task<CollectionExplorerViewModel> BuildSetExplorerAsync(string search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Products.AsNoTracking().Include(product => product.Brand).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(product => product.DisplayName.Contains(search) || product.ProductName.Contains(search) || product.Brand!.BrandName.Contains(search) || product.Year.ToString().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(product => product.Year)
            .ThenBy(product => product.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(product => new CollectionItemViewModel
            {
                Id = product.ProductId.ToString(),
                Title = product.DisplayName,
                Subtitle = product.Brand!.BrandName,
                Meta = product.Year.ToString(),
                LinkUrl = $"/CollectionExplorer/Detail?mode=sets&id={product.ProductId}"
            })
            .ToListAsync(cancellationToken);

        return new CollectionExplorerViewModel
        {
            Title = "Sets",
            Description = "Browse sets, check completion, and open set detail.",
            TotalCount = totalCount,
            Items = rows,
            SummaryMetrics = new[]
            {
                new DashboardMetricViewModel { Title = "Sets", Value = totalCount.ToString("N0", CultureInfo.CurrentCulture) },
                new DashboardMetricViewModel { Title = "Current Page", Value = page.ToString(CultureInfo.CurrentCulture) }
            }
        };
    }

    private async Task<CollectionExplorerViewModel> BuildPlayerExplorerAsync(string search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Cards.AsNoTracking().Where(card => card.Subject != null);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(card => card.Subject.Contains(search) || (card.Team != null && card.Team.Contains(search)));
        }

        var grouped = query.GroupBy(card => card.Subject)
            .Select(group => new
            {
                Id = group.Key,
                Title = group.Key,
                Subtitle = group.Select(item => item.Team).FirstOrDefault(),
                Quantity = group.Sum(item => item.Quantity),
                UniqueCount = group.Select(item => item.ChecklistItemId).Distinct().Count(),
                RookieCount = group.Count(item => item.IsRookie),
                TotalValue = group.Where(item => item.MyValue != null).Sum(item => item.MyValue ?? 0m)
            });

        var totalCount = await grouped.CountAsync(cancellationToken);
        var rows = await grouped.OrderByDescending(item => item.TotalValue).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new CollectionExplorerViewModel
        {
            Title = "Players",
            Description = "Browse cards by player or subject.",
            TotalCount = totalCount,
            Items = rows.Select(item => new CollectionItemViewModel
            {
                Id = item.Id,
                Title = item.Title,
                Subtitle = item.Subtitle,
                QuantityText = $"{item.Quantity:N0} pcs",
                Meta = $"{item.UniqueCount:N0} unique",
                Meta2 = $"{item.RookieCount:N0} rookies",
                ValueText = item.TotalValue > 0 ? item.TotalValue.ToString("C0", CultureInfo.CurrentCulture) : null,
                NumericValue = item.TotalValue,
                LinkUrl = $"/CollectionExplorer/Detail?mode=players&id={Uri.EscapeDataString(item.Id)}"
            }).ToList(),
            SummaryMetrics = new[] { new DashboardMetricViewModel { Title = "Players", Value = totalCount.ToString("N0", CultureInfo.CurrentCulture) } }
        };
    }

    private async Task<CollectionExplorerViewModel> BuildTeamExplorerAsync(string search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Cards.AsNoTracking().Where(card => card.Team != null);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(card => card.Team!.Contains(search));
        }

        var grouped = query.GroupBy(card => card.Team!)
            .Select(group => new
            {
                Id = group.Key,
                Title = group.Key,
                Quantity = group.Sum(item => item.Quantity),
                PlayerCount = group.Select(item => item.Subject).Distinct().Count(),
                TotalValue = group.Where(item => item.MyValue != null).Sum(item => item.MyValue ?? 0m)
            });

        var totalCount = await grouped.CountAsync(cancellationToken);
        var rows = await grouped.OrderByDescending(item => item.TotalValue).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new CollectionExplorerViewModel
        {
            Title = "Teams",
            Description = "Browse cards by team.",
            TotalCount = totalCount,
            Items = rows.Select(item => new CollectionItemViewModel
            {
                Id = item.Id,
                Title = item.Title,
                QuantityText = $"{item.Quantity:N0} pcs",
                Meta = $"{item.PlayerCount:N0} players",
                ValueText = item.TotalValue > 0 ? item.TotalValue.ToString("C0", CultureInfo.CurrentCulture) : null,
                NumericValue = item.TotalValue,
                LinkUrl = $"/CollectionExplorer/Detail?mode=teams&id={Uri.EscapeDataString(item.Id)}"
            }).ToList()
        };
    }

    private async Task<CollectionExplorerViewModel> BuildYearExplorerAsync(string search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Cards.AsNoTracking().Where(card => card.Year != null);
        if (!string.IsNullOrWhiteSpace(search) && int.TryParse(search, out var year))
        {
            query = query.Where(card => card.Year == year);
        }

        var grouped = query.GroupBy(card => card.Year!.Value)
            .Select(group => new
            {
                Id = group.Key,
                Quantity = group.Sum(item => item.Quantity),
                UniqueCount = group.Select(item => item.ChecklistItemId).Distinct().Count(),
                TotalValue = group.Where(item => item.MyValue != null).Sum(item => item.MyValue ?? 0m)
            });

        var totalCount = await grouped.CountAsync(cancellationToken);
        var rows = await grouped.OrderByDescending(item => item.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new CollectionExplorerViewModel
        {
            Title = "Years",
            Description = "Browse cards by year.",
            TotalCount = totalCount,
            Items = rows.Select(item => new CollectionItemViewModel
            {
                Id = item.Id.ToString(CultureInfo.CurrentCulture),
                Title = item.Id.ToString(CultureInfo.CurrentCulture),
                QuantityText = $"{item.Quantity:N0} pcs",
                Meta = $"{item.UniqueCount:N0} unique",
                ValueText = item.TotalValue > 0 ? item.TotalValue.ToString("C0", CultureInfo.CurrentCulture) : null,
                NumericValue = item.TotalValue,
                LinkUrl = $"/CollectionExplorer/Detail?mode=years&id={item.Id}"
            }).ToList()
        };
    }

    private async Task<CollectionExplorerViewModel> BuildManufacturerExplorerAsync(string search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Products.AsNoTracking().Include(product => product.Sport).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(product => product.Sport!.SportName.Contains(search));
        }

        var grouped = query.GroupBy(product => product.Sport!.SportName)
            .Select(group => new
            {
                Id = group.Key,
                SetCount = group.Count()
            });

        var totalCount = await grouped.CountAsync(cancellationToken);
        var rows = await grouped.OrderBy(item => item.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new CollectionExplorerViewModel
        {
            Title = "Manufacturers",
            Description = "Browse the catalog by manufacturer-like sport grouping.",
            TotalCount = totalCount,
            Items = rows.Select(item => new CollectionItemViewModel
            {
                Id = item.Id,
                Title = item.Id,
                Subtitle = "Manufacturer grouping is derived from the current sport catalog.",
                QuantityText = $"{item.SetCount:N0} sets",
                LinkUrl = $"/CollectionExplorer/Detail?mode=manufacturers&id={Uri.EscapeDataString(item.Id)}"
            }).ToList()
        };
    }

    private async Task<CollectionExplorerViewModel> BuildBrandExplorerAsync(string search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Brands.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(brand => brand.BrandName.Contains(search));
        }

        var grouped = query.Select(brand => new CollectionItemViewModel
        {
            Id = brand.BrandId.ToString(),
            Title = brand.BrandName,
            Subtitle = "Brand",
            QuantityText = $"{brand.Products.Count:N0} sets",
            LinkUrl = $"/CollectionExplorer/Detail?mode=brands&id={Uri.EscapeDataString(brand.BrandName)}"
        });

        var totalCount = await grouped.CountAsync(cancellationToken);
        var rows = await grouped.OrderBy(item => item.Title).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new CollectionExplorerViewModel { Title = "Brands", Description = "Browse cards by brand.", TotalCount = totalCount, Items = rows };
    }

    private async Task<CollectionExplorerViewModel> BuildChecklistSectionExplorerAsync(string search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.ChecklistItems.AsNoTracking().Include(item => item.Product).ThenInclude(product => product!.Brand).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(item => item.CardNumber.Contains(search) || item.Subject.Contains(search) || (item.Subset != null && item.Subset.Contains(search)));
        }

        var grouped = query.GroupBy(item => item.Subset ?? "Base")
            .Select(group => new CollectionItemViewModel
            {
                Id = group.Key,
                Title = group.Key,
                Subtitle = group.Select(item => item.Product!.DisplayName).FirstOrDefault(),
                QuantityText = $"{group.Count():N0} checklist cards",
                Meta = $"{group.Count(item => item.IsAutograph)} auto · {group.Count(item => item.IsRelic)} relic",
                LinkUrl = $"/CollectionExplorer/Detail?mode=checklistsections&id={Uri.EscapeDataString(group.Key)}"
            });

        var totalCount = await grouped.CountAsync(cancellationToken);
        var rows = await grouped.OrderBy(item => item.Title).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new CollectionExplorerViewModel { Title = "Checklist Sections", Description = "Browse the catalog by checklist section.", TotalCount = totalCount, Items = rows };
    }

    private async Task<CollectionExplorerViewModel> BuildFlagExplorerAsync(string title, Expression<Func<Card, bool>> predicate, string search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Cards.AsNoTracking().Include(card => card.Product).ThenInclude(product => product!.Brand).Where(predicate);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(card => card.Subject.Contains(search) || (card.Team != null && card.Team.Contains(search)) || (card.Set != null && card.Set.Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(card => card.ModifiedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(card => MapCard(card))
            .ToListAsync(cancellationToken);

        return new CollectionExplorerViewModel { Title = title, Description = $"Browse owned {title.ToLowerInvariant()}.", TotalCount = totalCount, Items = rows };
    }

    private async Task<CollectionExplorerViewModel> BuildParallelExplorerAsync(string search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Cards.AsNoTracking().Where(card => card.Variety != null || card.Set != null);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(card => (card.Variety != null && card.Variety.Contains(search)) || (card.Set != null && card.Set.Contains(search)));
        }

        var grouped = query.GroupBy(card => card.Variety ?? card.Set ?? "Parallel")
            .Select(group => new CollectionItemViewModel
            {
                Id = group.Key,
                Title = group.Key,
                QuantityText = $"{group.Sum(item => item.Quantity):N0} pcs",
                Meta = $"Max serial {group.Where(item => item.Serial != null).Select(item => item.Serial).FirstOrDefault() ?? ""}",
                LinkUrl = $"/CollectionExplorer/Detail?mode=parallels&id={Uri.EscapeDataString(group.Key)}"
            });

        var totalCount = await grouped.CountAsync(cancellationToken);
        var rows = await grouped.OrderBy(item => item.Title).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new CollectionExplorerViewModel { Title = "Parallels", Description = "Browse by parallel group.", TotalCount = totalCount, Items = rows };
    }

    private async Task<CollectionExplorerViewModel> BuildGradedExplorerAsync(string search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Cards.AsNoTracking().Include(card => card.Product).ThenInclude(product => product!.Brand).Where(card => card.GradeIssuer != null);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(card => card.Subject.Contains(search) || (card.GradeIssuer != null && card.GradeIssuer.Contains(search)) || (card.Grade != null && card.Grade.Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(card => card.ModifiedDate).Skip((page - 1) * pageSize).Take(pageSize).Select(card => MapCard(card)).ToListAsync(cancellationToken);
        return new CollectionExplorerViewModel { Title = "Graded Cards", Description = "Browse graded inventory.", TotalCount = totalCount, Items = rows };
    }

    private async Task<CollectionExplorerViewModel> BuildDuplicateExplorerAsync(string search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Cards.AsNoTracking().Where(card => card.ChecklistItemId != null);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(card => card.Subject.Contains(search) || (card.Team != null && card.Team.Contains(search)) || (card.CardNumber != null && card.CardNumber.Contains(search)));
        }

        var grouped = query.GroupBy(card => new { card.ChecklistItemId, card.Subject, card.CardNumber, card.Team })
            .Where(group => group.Count() > 1 || group.Sum(item => item.Quantity) > 1)
            .Select(group => new CollectionItemViewModel
            {
                Id = group.Key.ChecklistItemId.ToString()!,
                Title = group.Key.Subject,
                Subtitle = group.Key.CardNumber,
                Meta = group.Key.Team,
                QuantityText = $"{group.Sum(item => item.Quantity):N0} pcs",
                LinkUrl = $"/CollectionExplorer/Detail?mode=duplicates&id={Uri.EscapeDataString(group.Key.ChecklistItemId.ToString() ?? string.Empty)}"
            });

        var totalCount = await grouped.CountAsync(cancellationToken);
        var rows = await grouped.OrderByDescending(item => item.QuantityText).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new CollectionExplorerViewModel { Title = "Duplicates", Description = "Cards with multiple physical copies or duplicate checklist linkage.", TotalCount = totalCount, Items = rows };
    }

    private async Task<CollectionExplorerViewModel> BuildRecentlyAddedExplorerAsync(string search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Cards.AsNoTracking().Include(card => card.Product).ThenInclude(product => product!.Brand).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(card => card.Subject.Contains(search) || (card.Team != null && card.Team.Contains(search)) || (card.Set != null && card.Set.Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(card => card.CreatedDate).Skip((page - 1) * pageSize).Take(pageSize).Select(card => MapCard(card)).ToListAsync(cancellationToken);
        return new CollectionExplorerViewModel { Title = "Recently Added", Description = "Latest inventory additions.", TotalCount = totalCount, Items = rows };
    }

    private async Task<CollectionExplorerViewModel> BuildForSaleExplorerAsync(string search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Cards.AsNoTracking().Where(card => !string.IsNullOrWhiteSpace(card.ListingStatus) && card.ListingStatus != "Not Listed");
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(card => card.Subject.Contains(search) || (card.Team != null && card.Team.Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(card => card.ModifiedDate).Skip((page - 1) * pageSize).Take(pageSize).Select(card => MapCard(card)).ToListAsync(cancellationToken);
        return new CollectionExplorerViewModel { Title = "Cards for Sale", Description = "Inventory currently marked for sale.", TotalCount = totalCount, Items = rows };
    }

    private async Task<CollectionExplorerViewModel> BuildUnmatchedExplorerAsync(string search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Cards.AsNoTracking().Where(card => card.ChecklistItemId == null || card.ProductId == null);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(card => card.Subject.Contains(search) || (card.Set != null && card.Set.Contains(search)) || (card.CardNumber != null && card.CardNumber.Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(card => card.ModifiedDate).Skip((page - 1) * pageSize).Take(pageSize).Select(card => MapCard(card)).ToListAsync(cancellationToken);
        return new CollectionExplorerViewModel { Title = "Unmatched Cards", Description = "Cards not linked to a checklist item.", TotalCount = totalCount, Items = rows };
    }

    private async Task<CollectionExplorerDetailViewModel> BuildSetDetailAsync(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var productId))
        {
            return new CollectionExplorerDetailViewModel { Title = "Set detail", EmptyMessage = "Invalid set identifier." };
        }

        var product = await _context.Products.AsNoTracking().Include(product => product.Brand).FirstOrDefaultAsync(product => product.ProductId == productId, cancellationToken);
        if (product == null)
        {
            return new CollectionExplorerDetailViewModel { Title = "Set detail", EmptyMessage = "Set not found." };
        }

        var items = await _context.ChecklistItems.AsNoTracking().Where(item => item.ProductId == productId).OrderBy(item => item.CardNumber).Take(250).Select(item => new CollectionItemViewModel { Id = item.ChecklistItemId.ToString(), Title = item.Subject, Subtitle = item.CardNumber, Meta = item.Team, Badge = item.IsAutograph ? "Auto" : null, SecondaryBadge = item.IsRelic ? "Relic" : null }).ToListAsync(cancellationToken);
        return new CollectionExplorerDetailViewModel { Mode = "sets", Title = product.DisplayName, Subtitle = product.Brand?.BrandName, Items = items, SummaryMetrics = new[] { new DashboardMetricViewModel { Title = "Checklist Cards", Value = items.Count.ToString("N0", CultureInfo.CurrentCulture) } } };
    }

    private async Task<CollectionExplorerDetailViewModel> BuildPlayerDetailAsync(string id, CancellationToken cancellationToken)
    {
        var query = _context.Cards.AsNoTracking().Where(card => card.Subject == id);
        var items = await query.OrderByDescending(card => card.Year).ThenBy(card => card.CardNumber).Take(250).Select(card => MapCard(card)).ToListAsync(cancellationToken);
        return new CollectionExplorerDetailViewModel { Mode = "players", Title = id, Items = items, SummaryMetrics = new[] { new DashboardMetricViewModel { Title = "Cards", Value = items.Count.ToString("N0", CultureInfo.CurrentCulture) } } };
    }

    private async Task<CollectionExplorerDetailViewModel> BuildTeamDetailAsync(string id, CancellationToken cancellationToken)
    {
        var query = _context.Cards.AsNoTracking().Where(card => card.Team == id);
        var items = await query.OrderByDescending(card => card.Year).ThenBy(card => card.Subject).Take(250).Select(card => MapCard(card)).ToListAsync(cancellationToken);
        return new CollectionExplorerDetailViewModel { Mode = "teams", Title = id, Items = items, SummaryMetrics = new[] { new DashboardMetricViewModel { Title = "Cards", Value = items.Count.ToString("N0", CultureInfo.CurrentCulture) } } };
    }

    private async Task<CollectionExplorerDetailViewModel> BuildYearDetailAsync(string id, CancellationToken cancellationToken)
    {
        if (!int.TryParse(id, out var year))
        {
            return new CollectionExplorerDetailViewModel { Title = "Year detail", EmptyMessage = "Invalid year." };
        }

        var query = _context.Cards.AsNoTracking().Where(card => card.Year == year);
        var items = await query.OrderBy(card => card.Subject).Take(250).Select(card => MapCard(card)).ToListAsync(cancellationToken);
        return new CollectionExplorerDetailViewModel { Mode = "years", Title = year.ToString(CultureInfo.CurrentCulture), Items = items, SummaryMetrics = new[] { new DashboardMetricViewModel { Title = "Cards", Value = items.Count.ToString("N0", CultureInfo.CurrentCulture) } } };
    }

    private async Task<CollectionExplorerDetailViewModel> BuildManufacturerDetailAsync(string id, CancellationToken cancellationToken)
    {
        var query = _context.Cards.AsNoTracking().Include(card => card.Product).ThenInclude(product => product!.Brand).Where(card => card.Product != null && card.Product.Sport != null && card.Product.Sport.SportName == id);
        var items = await query.OrderByDescending(card => card.Year).Take(250).Select(card => MapCard(card)).ToListAsync(cancellationToken);
        return new CollectionExplorerDetailViewModel { Mode = "manufacturers", Title = id, Items = items, SummaryMetrics = new[] { new DashboardMetricViewModel { Title = "Cards", Value = items.Count.ToString("N0", CultureInfo.CurrentCulture) } } };
    }

    private async Task<CollectionExplorerDetailViewModel> BuildBrandDetailAsync(string id, CancellationToken cancellationToken)
    {
        var query = _context.Cards.AsNoTracking().Include(card => card.Product).ThenInclude(product => product!.Brand).Where(card => card.Product != null && card.Product.Brand != null && card.Product.Brand.BrandName == id);
        var items = await query.OrderByDescending(card => card.Year).Take(250).Select(card => MapCard(card)).ToListAsync(cancellationToken);
        return new CollectionExplorerDetailViewModel { Mode = "brands", Title = id, Items = items, SummaryMetrics = new[] { new DashboardMetricViewModel { Title = "Cards", Value = items.Count.ToString("N0", CultureInfo.CurrentCulture) } } };
    }

    private static CollectionItemViewModel MapCard(Card card) => new()
    {
        Id = card.CardId.ToString(),
        Title = card.Subject,
        Subtitle = card.Product?.DisplayName ?? card.Set,
        Meta = card.Year?.ToString(CultureInfo.CurrentCulture),
        Meta2 = card.Team,
        ImageUrl = card.FrontImagePath,
        LinkUrl = $"/Cards/Details/{card.CardId}",
        Badge = string.IsNullOrWhiteSpace(card.Variety) ? null : card.Variety,
        SecondaryBadge = string.IsNullOrWhiteSpace(card.Variety) ? null : card.Variety,
        QuantityText = $"Qty {card.Quantity}",
        ValueText = card.MyValue.HasValue ? card.MyValue.Value.ToString("C0", CultureInfo.CurrentCulture) : null,
        StorageText = string.Join(" / ", new[] { card.StorageBin, card.StorageBox, card.StorageRow }.Where(part => !string.IsNullOrWhiteSpace(part))),
        Grade = string.IsNullOrWhiteSpace(card.GradeIssuer) ? null : $"{card.GradeIssuer} {card.Grade}".Trim(),
        StatusText = card.ListingStatus
    };

    private static List<ExplorerModeOptionViewModel> BuildModeOptions(string mode, string search, string? normalizedSort, int page, int pageSize)
    {
        string Url(string code) => $"/CollectionExplorer?mode={code}&search={Uri.EscapeDataString(search)}&sort={Uri.EscapeDataString(normalizedSort ?? string.Empty)}&page={page}&pageSize={pageSize}";
        return new List<ExplorerModeOptionViewModel>
        {
            new() { Code = "sets", DisplayName = "Sets", Url = Url("sets"), IsActive = mode == "sets" },
            new() { Code = "players", DisplayName = "Players", Url = Url("players"), IsActive = mode == "players" },
            new() { Code = "teams", DisplayName = "Teams", Url = Url("teams"), IsActive = mode == "teams" },
            new() { Code = "years", DisplayName = "Years", Url = Url("years"), IsActive = mode == "years" },
            new() { Code = "manufacturers", DisplayName = "Manufacturers", Url = Url("manufacturers"), IsActive = mode == "manufacturers" },
            new() { Code = "brands", DisplayName = "Brands", Url = Url("brands"), IsActive = mode == "brands" },
            new() { Code = "checklistsections", DisplayName = "Checklist Sections", Url = Url("checklistsections"), IsActive = mode == "checklistsections" },
            new() { Code = "autographs", DisplayName = "Autographs", Url = Url("autographs"), IsActive = mode == "autographs" },
            new() { Code = "relics", DisplayName = "Relics", Url = Url("relics"), IsActive = mode == "relics" },
            new() { Code = "parallels", DisplayName = "Parallels", Url = Url("parallels"), IsActive = mode == "parallels" },
            new() { Code = "gradedcards", DisplayName = "Graded Cards", Url = Url("gradedcards"), IsActive = mode == "gradedcards" },
            new() { Code = "wishlist", DisplayName = "Wishlist", Url = Url("wishlist"), IsActive = mode == "wishlist" },
            new() { Code = "duplicates", DisplayName = "Duplicates", Url = Url("duplicates"), IsActive = mode == "duplicates" },
            new() { Code = "recentlyadded", DisplayName = "Recently Added", Url = Url("recentlyadded"), IsActive = mode == "recentlyadded" },
            new() { Code = "cardsforsale", DisplayName = "Cards for Sale", Url = Url("cardsforsale"), IsActive = mode == "cardsforsale" },
            new() { Code = "unmatchedcards", DisplayName = "Unmatched Cards", Url = Url("unmatchedcards"), IsActive = mode == "unmatchedcards" }
        };
    }

    private static List<CollectionExplorerFilterViewModel> BuildFilters(string mode, string search, string? sort, int page, int pageSize)
    {
        var baseUrl = "/CollectionExplorer";
        return new List<CollectionExplorerFilterViewModel>
        {
            new() { Name = "Search", Value = search, Url = $"{baseUrl}?mode={mode}&page=1&pageSize={pageSize}&sort={Uri.EscapeDataString(sort ?? string.Empty)}" },
            new() { Name = "Page", Value = page.ToString(CultureInfo.CurrentCulture), IsActive = true },
            new() { Name = "Page Size", Value = pageSize.ToString(CultureInfo.CurrentCulture), IsActive = true }
        };
    }

    private static string NormalizeMode(string? mode)
    {
        var normalized = (mode ?? string.Empty).Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? "sets" : normalized;
    }

    private CollectionExplorerViewModel BuildUnavailableExplorerLegacy(string title, string message)
    {
        return new CollectionExplorerViewModel
        {
            Title = title,
            Description = message,
            EmptyMessage = message,
            IsUnavailable = true,
            Items = Array.Empty<CollectionItemViewModel>()
        };
    }
}

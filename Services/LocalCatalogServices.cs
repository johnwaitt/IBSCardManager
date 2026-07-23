using IBSCardManager.Data;
using IBSCardManager.Entities;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Services;

public sealed class LocalCatalogDatabaseProvider : ICatalogDatabaseProvider
{
    private readonly ApplicationDbContext _context;

    public LocalCatalogDatabaseProvider(ApplicationDbContext context)
    {
        _context = context;
    }

    public string ProviderName => "LocalSqlServerCatalogTables";
    public string DatabaseRole => "RuntimePrimary";

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        return _context.Database.CanConnectAsync(cancellationToken);
    }
}

public sealed class LocalMasterCatalogService : IMasterCatalogService
{
    private readonly ApplicationDbContext _context;

    public LocalMasterCatalogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Product?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return _context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);
    }

    public Task<ChecklistItem?> GetChecklistItemAsync(Guid checklistItemId, CancellationToken cancellationToken = default)
    {
        return _context.ChecklistItems.AsNoTracking().FirstOrDefaultAsync(x => x.ChecklistItemId == checklistItemId, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> SearchProductsAsync(string? search, int take = 50, CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.DisplayName.Contains(search) || x.ProductName.Contains(search));
        }

        return await query
            .OrderByDescending(x => x.Year)
            .ThenBy(x => x.DisplayName)
            .Take(Math.Clamp(take, 1, 500))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ChecklistItem>> SearchChecklistItemsAsync(Guid? productId, string? search, int take = 100, CancellationToken cancellationToken = default)
    {
        var query = _context.ChecklistItems.AsNoTracking().AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(x => x.ProductId == productId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Subject.Contains(search) || x.CardNumber.Contains(search) || (x.Team != null && x.Team.Contains(search)));
        }

        return await query
            .OrderBy(x => x.CardNumber)
            .ThenBy(x => x.Subject)
            .Take(Math.Clamp(take, 1, 1000))
            .ToListAsync(cancellationToken);
    }
}

public sealed class LocalCatalogLookupService : ICatalogLookupService
{
    private readonly ApplicationDbContext _context;

    public LocalCatalogLookupService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Product?> FindProductByStableIdAsync(string catalogRecordId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(catalogRecordId))
        {
            return Task.FromResult<Product?>(null);
        }

        return _context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.CatalogRecordId == catalogRecordId, cancellationToken);
    }

    public Task<ChecklistItem?> FindChecklistCardByStableIdAsync(string catalogRecordId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(catalogRecordId))
        {
            return Task.FromResult<ChecklistItem?>(null);
        }

        return _context.ChecklistItems.AsNoTracking().FirstOrDefaultAsync(x => x.CatalogRecordId == catalogRecordId, cancellationToken);
    }
}

public sealed class LocalCatalogSearchService : ICatalogSearchService
{
    private readonly IMasterCatalogService _masterCatalog;

    public LocalCatalogSearchService(IMasterCatalogService masterCatalog)
    {
        _masterCatalog = masterCatalog;
    }

    public Task<IReadOnlyList<Product>> SearchProductsAsync(string? query, int take = 50, CancellationToken cancellationToken = default)
    {
        return _masterCatalog.SearchProductsAsync(query, take, cancellationToken);
    }

    public Task<IReadOnlyList<ChecklistItem>> SearchChecklistCardsAsync(string? query, int take = 100, CancellationToken cancellationToken = default)
    {
        return _masterCatalog.SearchChecklistItemsAsync(null, query, take, cancellationToken);
    }
}

public sealed class LocalCatalogVersionService : ICatalogVersionService
{
    private readonly ApplicationDbContext _context;

    public LocalCatalogVersionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetCatalogVersionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ChecklistItems.AsNoTracking()
            .Where(x => x.CatalogVersion != null)
            .OrderByDescending(x => x.CatalogUpdatedAt)
            .Select(x => x.CatalogVersion)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DateTime?> GetCatalogUpdatedAtAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ChecklistItems.AsNoTracking()
            .Where(x => x.CatalogUpdatedAt != null)
            .OrderByDescending(x => x.CatalogUpdatedAt)
            .Select(x => x.CatalogUpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public sealed class LocalCatalogImportService : ICatalogImportService
{
    public Task<CatalogImportResult> ImportAsync(CatalogImportRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CatalogImportResult
        {
            Success = true,
            Message = "Catalog import boundary registered. Current provider uses local catalog tables.",
            ImportedProducts = 0,
            ImportedChecklistCards = 0
        });
    }
}

public sealed class LocalCatalogImageService : ICatalogImageService
{
    private readonly ApplicationDbContext _context;

    public LocalCatalogImageService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<string?> GetFrontImageUrlAsync(Guid checklistItemId, CancellationToken cancellationToken = default)
    {
        return _context.ChecklistItems.AsNoTracking()
            .Where(x => x.ChecklistItemId == checklistItemId)
            .Select(x => x.ReferenceImageUrl ?? x.StockImageUrl)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<string?> GetBackImageUrlAsync(Guid checklistItemId, CancellationToken cancellationToken = default)
    {
        return _context.ChecklistItems.AsNoTracking()
            .Where(x => x.ChecklistItemId == checklistItemId)
            .Select(x => x.StockBackImageUrl)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

using IBSCardManager.Data;
using IBSCardManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Controllers;

public class CollectionController : Controller
{
    private readonly ApplicationDbContext _context;

    public CollectionController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var productQuery = _context.Products
            .AsNoTracking()
            .Include(p => p.Brand)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            productQuery = productQuery.Where(p =>
                p.DisplayName.Contains(term) ||
                p.ProductName.Contains(term) ||
                (p.Brand != null && p.Brand.BrandName.Contains(term)) ||
                p.Year.ToString().Contains(term));
        }

        var products = await productQuery
            .OrderByDescending(p => p.Year)
            .ThenBy(p => p.DisplayName)
            .ToListAsync();

        var productIds = products.Select(p => p.ProductId).ToList();

        var checklistCounts = await _context.ChecklistItems
            .AsNoTracking()
            .Where(x => productIds.Contains(x.ProductId))
            .GroupBy(x => x.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProductId, x => x.Count);

        var inventory = await _context.Cards
            .AsNoTracking()
            .Where(c => c.ProductId.HasValue && productIds.Contains(c.ProductId.Value))
            .GroupBy(c => c.ProductId!.Value)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalQuantity = g.Sum(c => c.Quantity),
                UniqueOwned = g.Where(c => c.ChecklistItemId != null && c.Quantity > 0)
                    .Select(c => c.ChecklistItemId)
                    .Distinct()
                    .Count()
            })
            .ToDictionaryAsync(x => x.ProductId);

        var rows = products.Select(product =>
        {
            inventory.TryGetValue(product.ProductId, out var owned);
            return new CollectionSetSummary
            {
                ProductId = product.ProductId,
                Year = product.Year,
                ProductName = product.ProductName,
                DisplayName = product.DisplayName,
                BrandName = product.Brand?.BrandName ?? string.Empty,
                ChecklistCount = checklistCounts.GetValueOrDefault(product.ProductId),
                UniqueOwned = owned?.UniqueOwned ?? 0,
                TotalQuantity = owned?.TotalQuantity ?? 0
            };
        }).ToList();

        var model = new CollectionOverviewViewModel
        {
            Search = search?.Trim() ?? string.Empty,
            TotalSets = rows.Count,
            TotalChecklistCards = rows.Sum(x => x.ChecklistCount),
            TotalUniqueOwned = rows.Sum(x => x.UniqueOwned),
            TotalPiecesOwned = rows.Sum(x => x.TotalQuantity),
            Sets = rows
        };

        return View(model);
    }
}

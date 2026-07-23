using IBSCardManager.Data;
using IBSCardManager.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Controllers;

public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;
    public ProductsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(string? search, int? year)
    {
        var query = _context.Products.Include(p => p.Brand).Include(p => p.Sport)
            .Include(p => p.ChecklistItems).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.DisplayName.Contains(search) || p.ProductName.Contains(search));
        if (year.HasValue) query = query.Where(p => p.Year == year.Value);
        ViewBag.Search = search; ViewBag.Year = year;
        return View(await query.OrderByDescending(p => p.Year).ThenBy(p => p.DisplayName).ToListAsync());
    }

    public async Task<IActionResult> Create() { await LoadLists(); return View(new Product { Year = DateTime.Today.Year, IsActive = true }); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        await SetDisplayName(product);
        if (!ModelState.IsValid) { await LoadLists(product); return View(product); }
        _context.Products.Add(product); await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var product = await _context.Products.FindAsync(id); if (product == null) return NotFound();
        await LoadLists(product); return View(product);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Product product)
    {
        if (id != product.ProductId) return NotFound();
        await SetDisplayName(product);
        if (!ModelState.IsValid) { await LoadLists(product); return View(product); }
        _context.Update(product); await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var product = await _context.Products.Include(p => p.ChecklistItems).FirstOrDefaultAsync(p => p.ProductId == id);
        if (product == null) return NotFound();
        if (await _context.Cards.AnyAsync(c => c.ProductId == id))
        { TempData["Error"] = "This set is used by inventory cards and cannot be deleted."; return RedirectToAction(nameof(Index)); }
        _context.Products.Remove(product); await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLists(Product? product = null)
    {
        ViewBag.Sports = new SelectList(await _context.Sports.Where(x => x.IsActive).OrderBy(x => x.SportName).ToListAsync(), "SportId", "SportName", product?.SportId);
        ViewBag.Brands = new SelectList(await _context.Brands.Where(x => x.IsActive).OrderBy(x => x.BrandName).ToListAsync(), "BrandId", "BrandName", product?.BrandId);
    }

    private async Task SetDisplayName(Product product)
    {
        var brand = await _context.Brands.AsNoTracking().FirstOrDefaultAsync(x => x.BrandId == product.BrandId);
        if (brand == null) { ModelState.AddModelError(nameof(product.BrandId), "Select a valid brand."); return; }
        product.DisplayName = $"{product.Year} {brand.BrandName} {product.ProductName}".Trim();
        ModelState.Remove(nameof(product.DisplayName));
    }
}

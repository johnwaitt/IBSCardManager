using IBSCardManager.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Controllers;

public sealed class CatalogImportHistoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public CatalogImportHistoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var history = await _context.ChecklistImportHistories
            .AsNoTracking()
            .Include(x => x.Product)
            .OrderByDescending(x => x.ImportedUtc)
            .Take(250)
            .ToListAsync(cancellationToken);

        return View(history);
    }
}

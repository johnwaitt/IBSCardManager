using IBSCardManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace IBSCardManager.Controllers;

public class CollectionExplorerController : Controller
{
    private readonly ICollectionAnalyticsService _service;

    public CollectionExplorerController(ICollectionAnalyticsService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(string? mode, string? search, int page = 1, int pageSize = 24, string? sort = null, CancellationToken cancellationToken = default)
    {
        var model = await _service.BuildExplorerAsync(mode ?? "sets", search, page, pageSize, sort, cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> Detail(string mode, string id, CancellationToken cancellationToken = default)
    {
        var model = await _service.BuildDetailAsync(mode, id, cancellationToken);
        return View(model);
    }
}

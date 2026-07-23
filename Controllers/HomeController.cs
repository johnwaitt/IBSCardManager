using IBSCardManager.Models;
using IBSCardManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace IBSCardManager.Controllers;

public class HomeController : Controller
{
    private readonly ICollectionAnalyticsService _analyticsService;
    private readonly IDiagnosticsService _diagnosticsService;
    private readonly IBackupManifestService _backupManifestService;
    private readonly IApplicationVersionProvider _applicationVersionProvider;

    public HomeController(
        ICollectionAnalyticsService analyticsService,
        IDiagnosticsService diagnosticsService,
        IBackupManifestService backupManifestService,
        IApplicationVersionProvider applicationVersionProvider)
    {
        _analyticsService = analyticsService;
        _diagnosticsService = diagnosticsService;
        _backupManifestService = backupManifestService;
        _applicationVersionProvider = applicationVersionProvider;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _analyticsService.BuildDashboardAsync(cancellationToken);
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult About()
    {
        ViewData["Version"] = _applicationVersionProvider.ApplicationVersion;
        ViewData["InformationalVersion"] = _applicationVersionProvider.InformationalVersion;
        return View();
    }

    public async Task<IActionResult> Diagnostics(CancellationToken cancellationToken)
    {
        var model = await _diagnosticsService.BuildDiagnosticsAsync(cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> BackupManifest(CancellationToken cancellationToken)
    {
        var model = await _backupManifestService.GenerateManifestAsync(cancellationToken);
        return View(model);
    }
}

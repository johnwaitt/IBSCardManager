using IBSCardManager.Services;
using IBSCardManager.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IBSCardManager.Controllers;

public sealed class CatalogImportController : Controller
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".csv", ".json", ".xlsx" };

    private const long MaximumUploadBytes = 25 * 1024 * 1024;
    private readonly ICatalogImportService _catalogImportService;
    private readonly IWebHostEnvironment _environment;

    public CatalogImportController(
        ICatalogImportService catalogImportService,
        IWebHostEnvironment environment)
    {
        _catalogImportService = catalogImportService;
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new CatalogImportViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaximumUploadBytes)]
    public async Task<IActionResult> Index(
        CatalogImportViewModel model,
        CancellationToken cancellationToken)
    {
        if (model.CatalogFile is null || model.CatalogFile.Length == 0)
        {
            model.ErrorMessage = "Select a CSV, JSON, or Excel catalog file.";
            return View(model);
        }

        if (model.CatalogFile.Length > MaximumUploadBytes)
        {
            model.ErrorMessage = "The catalog file must be 25 MB or smaller.";
            return View(model);
        }

        var extension = Path.GetExtension(model.CatalogFile.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            model.ErrorMessage = "Only .csv, .json, and .xlsx files are supported.";
            return View(model);
        }

        var sourceName = string.IsNullOrWhiteSpace(model.SourceName)
            ? Path.GetFileNameWithoutExtension(model.CatalogFile.FileName)
            : model.SourceName.Trim();

        var importDirectory = Path.Combine(
            _environment.ContentRootPath,
            "App_Data",
            "CatalogImports");
        Directory.CreateDirectory(importDirectory);

        var temporaryPath = Path.Combine(
            importDirectory,
            $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}");

        try
        {
            await using (var output = System.IO.File.Create(temporaryPath))
            {
                await model.CatalogFile.CopyToAsync(output, cancellationToken);
            }

            model.Result = await _catalogImportService.ImportAsync(
                new CatalogImportRequest
                {
                    SourceName = sourceName,
                    SourceVersion = model.SourceVersion?.Trim(),
                    SourceLocation = temporaryPath
                },
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            model.ErrorMessage = $"Import failed: {ex.Message}";
        }
        finally
        {
            if (System.IO.File.Exists(temporaryPath))
            {
                System.IO.File.Delete(temporaryPath);
            }
        }

        ModelState.Clear();
        return View(model);
    }
}

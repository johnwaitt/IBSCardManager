using System.Text;
using System.Text.Json;
using IBSCardManager.Data;
using IBSCardManager.Entities;
using IBSCardManager.Models;
using IBSCardManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Controllers;

public sealed class AnalyticsController : Controller
{
    private readonly ICollectionInsightsService _insights;
    private readonly ApplicationDbContext _context;
    private readonly IAnalyticsRecalculationQueue _queue;

    public AnalyticsController(ICollectionInsightsService insights, ApplicationDbContext context, IAnalyticsRecalculationQueue queue)
    {
        _insights = insights;
        _context = context;
        _queue = queue;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string range = "all", CancellationToken cancellationToken = default)
    {
        var model = await _insights.BuildCollectionAnalyticsDashboardAsync(range, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Snapshot(CancellationToken cancellationToken = default)
    {
        var result = await _insights.CreateSnapshotAsync("manual-request", cancellationToken);
        TempData["AnalyticsMessage"] = result.Reason;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Recommendations(CancellationToken cancellationToken = default)
    {
        var model = await _insights.BuildRecommendationCenterAsync(cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Preferences(CancellationToken cancellationToken = default)
    {
        var preference = await _context.UserAnalyticsPreferences.AsNoTracking()
            .OrderBy(x => x.ProfileName)
            .FirstOrDefaultAsync(cancellationToken)
            ?? new UserAnalyticsPreference();

        return View(new AnalyticsPreferenceViewModel
        {
            Id = preference.Id,
            ProfileName = preference.ProfileName,
            CollectionGoal = preference.CollectionGoal,
            MinimumCashFlowTarget = preference.MinimumCashFlowTarget,
            NotifyPriceIncrease = preference.NotifyPriceIncrease,
            NotifyPriceDecrease = preference.NotifyPriceDecrease,
            NotifyGradingOpportunity = preference.NotifyGradingOpportunity,
            NotifyStaleValuation = preference.NotifyStaleValuation,
            NotifyReadyToList = preference.NotifyReadyToList,
            NotifyListingSold = preference.NotifyListingSold,
            NotifyDuplicateInventory = preference.NotifyDuplicateInventory,
            NotifyMissingCostBasis = preference.NotifyMissingCostBasis,
            NotifyLowConfidenceIdentification = preference.NotifyLowConfidenceIdentification,
            ModifiedAt = preference.ModifiedAt
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preferences(AnalyticsPreferenceViewModel model, CancellationToken cancellationToken = default)
    {
        var existing = await _context.UserAnalyticsPreferences.FirstOrDefaultAsync(cancellationToken);
        if (existing is null)
        {
            existing = new UserAnalyticsPreference();
            _context.UserAnalyticsPreferences.Add(existing);
        }

        existing.ProfileName = string.IsNullOrWhiteSpace(model.ProfileName) ? "Default" : model.ProfileName.Trim();
        existing.CollectionGoal = string.IsNullOrWhiteSpace(model.CollectionGoal) ? "MaximizeProfit" : model.CollectionGoal.Trim();
        existing.MinimumCashFlowTarget = model.MinimumCashFlowTarget;
        existing.NotifyPriceIncrease = model.NotifyPriceIncrease;
        existing.NotifyPriceDecrease = model.NotifyPriceDecrease;
        existing.NotifyGradingOpportunity = model.NotifyGradingOpportunity;
        existing.NotifyStaleValuation = model.NotifyStaleValuation;
        existing.NotifyReadyToList = model.NotifyReadyToList;
        existing.NotifyListingSold = model.NotifyListingSold;
        existing.NotifyDuplicateInventory = model.NotifyDuplicateInventory;
        existing.NotifyMissingCostBasis = model.NotifyMissingCostBasis;
        existing.NotifyLowConfidenceIdentification = model.NotifyLowConfidenceIdentification;
        existing.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await _queue.EnqueueAsync("goal-change", cancellationToken);

        TempData["AnalyticsMessage"] = "Analytics preferences saved. Recalculation queued.";
        return RedirectToAction(nameof(Preferences));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(Guid id, CancellationToken cancellationToken = default)
    {
        await UpdateRecommendationStateAsync(id, accepted: true, dismissed: false, snoozedUntil: null, cancellationToken);
        await _queue.EnqueueAsync("recommendation-state-change", cancellationToken);
        return RedirectToAction(nameof(Recommendations));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Dismiss(Guid id, CancellationToken cancellationToken = default)
    {
        await UpdateRecommendationStateAsync(id, accepted: false, dismissed: true, snoozedUntil: null, cancellationToken);
        await _queue.EnqueueAsync("recommendation-state-change", cancellationToken);
        return RedirectToAction(nameof(Recommendations));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Snooze(Guid id, int days = 14, CancellationToken cancellationToken = default)
    {
        await UpdateRecommendationStateAsync(id, accepted: false, dismissed: false, snoozedUntil: DateTime.UtcNow.AddDays(Math.Clamp(days, 1, 90)), cancellationToken);
        await _queue.EnqueueAsync("recommendation-state-change", cancellationToken);
        return RedirectToAction(nameof(Recommendations));
    }

    [HttpGet]
    public async Task<IActionResult> Report(string code = "mostvaluable", CancellationToken cancellationToken = default)
    {
        ViewData["ReportCode"] = code;
        var rows = await _insights.GetTopReportAsync(code, 100, cancellationToken);
        return View(rows);
    }

    [HttpGet]
    public async Task<IActionResult> Concentration(string dimension = "player", CancellationToken cancellationToken = default)
    {
        ViewData["Dimension"] = dimension;
        var rows = await _insights.GetConcentrationAsync(dimension, cancellationToken);
        return View(rows);
    }

    [HttpGet]
    public async Task<IActionResult> Duplicates(CancellationToken cancellationToken = default)
    {
        var rows = await _insights.GetDuplicateAnalyticsAsync(cancellationToken);
        return View(rows);
    }

    [HttpGet]
    public async Task<IActionResult> DataQuality(CancellationToken cancellationToken = default)
    {
        var rows = await _insights.GetDataQualityIssuesAsync(cancellationToken);
        return View(rows);
    }

    [HttpGet]
    public async Task<IActionResult> Export(string kind = "summary", string format = "csv", CancellationToken cancellationToken = default)
    {
        format = format.ToLowerInvariant();
        kind = kind.ToLowerInvariant();

        if (kind == "summary")
        {
            var summary = await _insights.BuildCollectionAnalyticsDashboardAsync("all", cancellationToken);
            return ExportObject(summary, kind, format);
        }

        if (kind == "grading")
        {
            var rows = await _insights.GetTopReportAsync("mostvaluable", 200, cancellationToken);
            return ExportObject(rows, kind, format);
        }

        if (kind == "sell")
        {
            var rows = await _insights.GetTopReportAsync("highestgains", 200, cancellationToken);
            return ExportObject(rows, kind, format);
        }

        if (kind == "hold")
        {
            var rows = await _insights.GetTopReportAsync("lowestroi", 200, cancellationToken);
            return ExportObject(rows, kind, format);
        }

        if (kind == "quality")
        {
            var rows = await _insights.GetDataQualityIssuesAsync(cancellationToken);
            return ExportObject(rows, kind, format);
        }

        if (kind == "duplicates")
        {
            var rows = await _insights.GetDuplicateAnalyticsAsync(cancellationToken);
            return ExportObject(rows, kind, format);
        }

        if (kind == "concentration")
        {
            var rows = await _insights.GetConcentrationAsync("player", cancellationToken);
            return ExportObject(rows, kind, format);
        }

        return BadRequest("Unknown export kind.");
    }

    private IActionResult ExportObject<T>(T payload, string kind, string format)
    {
        if (format == "json")
        {
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            return File(Encoding.UTF8.GetBytes(json), "application/json", $"analytics-{kind}.json");
        }

        var csv = ToCsv(payload);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"analytics-{kind}.csv");
    }

    private static string ToCsv<T>(T payload)
    {
        if (payload is IEnumerable<object> rows)
        {
            var rowList = rows.ToList();
            if (rowList.Count == 0) return "";
            var properties = rowList[0].GetType().GetProperties();
            var header = string.Join(",", properties.Select(p => p.Name));
            var lines = new List<string> { header };
            lines.AddRange(rowList.Select(row => string.Join(",", properties.Select(p => CsvCell(p.GetValue(row))))));
            return string.Join(Environment.NewLine, lines);
        }

        var props = typeof(T).GetProperties();
        return string.Join(Environment.NewLine, props.Select(p => $"{p.Name},{CsvCell(p.GetValue(payload))}"));
    }

    private static string CsvCell(object? value)
    {
        var raw = value?.ToString() ?? string.Empty;
        raw = raw.Replace("\"", "\"\"");
        return raw.Contains(',') || raw.Contains('"') || raw.Contains('\n') ? $"\"{raw}\"" : raw;
    }

    private async Task UpdateRecommendationStateAsync(Guid id, bool accepted, bool dismissed, DateTime? snoozedUntil, CancellationToken cancellationToken)
    {
        var records = await _context.RecommendationRecords
            .Where(x => x.InventoryCardId == id)
            .OrderByDescending(x => x.GeneratedAt)
            .ToListAsync(cancellationToken);

        if (records.Count == 0)
        {
            return;
        }

        foreach (var record in records)
        {
            record.Accepted = accepted;
            record.Dismissed = dismissed;
            record.SnoozedUntil = snoozedUntil;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}

using IBSCardManager.Data;
using IBSCardManager.Models;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Services;

public sealed class ChecklistCandidateService : IChecklistCandidateService
{
    private readonly ApplicationDbContext _context;

    public ChecklistCandidateService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<IReadOnlyList<ChecklistImportPreviewRowViewModel>> BuildPreviewAsync(ChecklistImportPreviewRequest request, CancellationToken cancellationToken = default)
    {
        var rows = request.Rows.Select((row, index) => new ChecklistImportPreviewRowViewModel
        {
            RowNumber = index + 1,
            CardNumber = row.CardNumber,
            Subject = row.Subject,
            Team = row.Team,
            ChecklistSection = row.ChecklistSection,
            Parallel = row.Parallel,
            Variation = row.Variation,
            SourceRecordId = row.SourceRecordId,
            Notes = row.Notes,
            ValidationStatus = string.IsNullOrWhiteSpace(row.CardNumber) || string.IsNullOrWhiteSpace(row.Subject)
                ? "Missing required fields"
                : "Ready"
        }).ToList();

        return Task.FromResult<IReadOnlyList<ChecklistImportPreviewRowViewModel>>(rows);
    }

    public async Task<IReadOnlyList<ScannerCandidateResult>> FindLocalCandidatesAsync(ScannerStructuredSearchRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.ChecklistItems
            .AsNoTracking()
            .Include(item => item.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Player))
        {
            var player = request.Player.Trim();
            query = query.Where(item => item.Subject.Contains(player));
        }

        if (!string.IsNullOrWhiteSpace(request.Product))
        {
            var product = request.Product.Trim();
            query = query.Where(item => item.Product != null && item.Product.DisplayName.Contains(product));
        }

        if (!string.IsNullOrWhiteSpace(request.CardNumber))
        {
            var cardNumber = request.CardNumber.Trim();
            query = query.Where(item => item.CardNumber == cardNumber);
        }

        if (!string.IsNullOrWhiteSpace(request.Team))
        {
            var team = request.Team.Trim();
            query = query.Where(item => item.Team != null && item.Team.Contains(team));
        }

        if (request.Year.HasValue)
        {
            query = query.Where(item => item.Product != null && item.Product.Year == request.Year.Value);
        }

        var results = await query
            .OrderBy(item => item.Product!.DisplayName)
            .ThenBy(item => item.CardNumber)
            .Take(25)
            .Select(item => new ScannerCandidateResult
            {
                ChecklistItemId = item.ChecklistItemId,
                ProductId = item.ProductId,
                CatalogSource = "Checklist",
                MatchStatus = "Candidate",
                Player = item.Subject,
                Team = item.Team,
                Year = item.Product != null ? item.Product.Year : null,
                Brand = item.Product != null && item.Product.Brand != null ? item.Product.Brand.BrandName : null,
                Product = item.Product != null ? item.Product.DisplayName : null,
                ChecklistSection = item.Subset,
                CardNumber = item.CardNumber,
                Parallel = item.Parallel,
                Variation = item.Variation,
                IsRookie = item.IsRookie,
                IsAutograph = item.IsAutograph,
                IsRelic = item.IsRelic,
                Confidence = 0.5m,
                MatchReasons = new[] { "Structured local checklist query" }
            })
            .ToListAsync(cancellationToken);

        return results;
    }
}

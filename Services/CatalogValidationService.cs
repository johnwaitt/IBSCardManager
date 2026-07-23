using IBSCardManager.Data;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Services;

public sealed class CatalogValidationService : ICatalogValidationService
{
    private readonly ApplicationDbContext _context;

    public CatalogValidationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CatalogIntegrityIssue>> RunReadinessChecksAsync(CancellationToken cancellationToken = default)
    {
        var issues = new List<CatalogIntegrityIssue>();

        var missingCatalogRefs = await _context.Cards.AsNoTracking()
            .CountAsync(card => card.ChecklistItemId == null && card.ProductId != null, cancellationToken);

        if (missingCatalogRefs > 0)
        {
            issues.Add(new CatalogIntegrityIssue
            {
                Code = "INVENTORY_MISSING_CHECKLIST_RELATION",
                Severity = "Warning",
                Summary = "Inventory cards reference products without checklist card linkage.",
                Details = $"Count: {missingCatalogRefs}"
            });
        }

        var missingStableIds = await _context.ChecklistItems.AsNoTracking()
            .CountAsync(item => item.CatalogRecordId == null, cancellationToken);

        if (missingStableIds > 0)
        {
            issues.Add(new CatalogIntegrityIssue
            {
                Code = "CATALOG_MISSING_STABLE_ID",
                Severity = "Info",
                Summary = "Checklist cards missing CatalogRecordId.",
                Details = $"Count: {missingStableIds}"
            });
        }

        var duplicateCatalogIds = await _context.ChecklistItems.AsNoTracking()
            .Where(item => item.CatalogRecordId != null)
            .GroupBy(item => item.CatalogRecordId)
            .Where(group => group.Count() > 1)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        foreach (var duplicate in duplicateCatalogIds.Take(10))
        {
            issues.Add(new CatalogIntegrityIssue
            {
                Code = "DUPLICATE_CATALOG_IDENTIFIER",
                Severity = "Warning",
                Summary = "Duplicate checklist CatalogRecordId detected.",
                Details = $"CatalogRecordId={duplicate.Key}; Count={duplicate.Count}"
            });
        }

        var deprecatedInUse = await _context.Cards.AsNoTracking()
            .Join(_context.ChecklistItems.AsNoTracking(),
                card => card.ChecklistItemId,
                item => (Guid?)item.ChecklistItemId,
                (card, item) => new { card.CardId, item.IsDeprecated })
            .CountAsync(link => link.IsDeprecated, cancellationToken);

        if (deprecatedInUse > 0)
        {
            issues.Add(new CatalogIntegrityIssue
            {
                Code = "DEPRECATED_CATALOG_REFERENCED",
                Severity = "Warning",
                Summary = "Inventory references deprecated checklist records.",
                Details = $"Count: {deprecatedInUse}"
            });
        }

        var duplicateProductNames = await _context.Products.AsNoTracking()
            .GroupBy(product => product.DisplayName)
            .Where(group => group.Count() > 1)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        foreach (var duplicate in duplicateProductNames.Take(10))
        {
            issues.Add(new CatalogIntegrityIssue
            {
                Code = "DUPLICATE_PRODUCT_NAME_DIFFERENT_IDS",
                Severity = "Info",
                Summary = "Duplicate product display names detected.",
                Details = $"DisplayName={duplicate.Key}; Count={duplicate.Count}"
            });
        }

        var duplicateChecklistWithinProduct = await _context.ChecklistItems.AsNoTracking()
            .GroupBy(item => new { item.ProductId, item.CardNumber, item.Parallel, item.Variation })
            .Where(group => group.Count() > 1)
            .Select(group => new { group.Key.ProductId, group.Key.CardNumber, group.Key.Parallel, group.Key.Variation, Count = group.Count() })
            .ToListAsync(cancellationToken);

        foreach (var duplicate in duplicateChecklistWithinProduct.Take(10))
        {
            issues.Add(new CatalogIntegrityIssue
            {
                Code = "DUPLICATE_CHECKLIST_WITHIN_PRODUCT",
                Severity = "Warning",
                Summary = "Duplicate checklist cards within same product.",
                Details = $"ProductId={duplicate.ProductId}; CardNumber={duplicate.CardNumber}; Parallel={duplicate.Parallel}; Variation={duplicate.Variation}; Count={duplicate.Count}"
            });
        }

        var conflictingCardNumbers = await _context.ChecklistItems.AsNoTracking()
            .GroupBy(item => new { item.ProductId, item.CardNumber })
            .Where(group => group.Select(item => item.Subject).Distinct().Count() > 1)
            .Select(group => new { group.Key.ProductId, group.Key.CardNumber, SubjectCount = group.Select(item => item.Subject).Distinct().Count() })
            .ToListAsync(cancellationToken);

        foreach (var conflict in conflictingCardNumbers.Take(10))
        {
            issues.Add(new CatalogIntegrityIssue
            {
                Code = "CONFLICTING_CARD_NUMBERS",
                Severity = "Warning",
                Summary = "Conflicting subjects share card number within product.",
                Details = $"ProductId={conflict.ProductId}; CardNumber={conflict.CardNumber}; SubjectCount={conflict.SubjectCount}"
            });
        }

        return issues;
    }
}

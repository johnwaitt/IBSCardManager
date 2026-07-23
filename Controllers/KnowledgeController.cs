using IBSCardManager.Entities;
using IBSCardManager.Models;
using IBSCardManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Controllers;

public sealed class KnowledgeController : Controller
{
    private readonly IKnowledgeQueryService _knowledgeQueryService;
    private readonly IKnowledgeEvidenceService _knowledgeEvidenceService;
    private readonly IBSCardManager.Data.ApplicationDbContext _context;

    public KnowledgeController(
        IKnowledgeQueryService knowledgeQueryService,
        IKnowledgeEvidenceService knowledgeEvidenceService,
        IBSCardManager.Data.ApplicationDbContext context)
    {
        _knowledgeQueryService = knowledgeQueryService;
        _knowledgeEvidenceService = knowledgeEvidenceService;
        _context = context;
    }

    [HttpGet]
    public IActionResult ReviewQueue(int page = 1, int pageSize = 25)
    {
        var take = Math.Clamp(pageSize, 10, 100);
        var skip = Math.Max(0, (page - 1) * take);

        var query = _context.KnowledgeReviewItems.AsQueryable().OrderByDescending(x => x.CreatedAt);
        var total = query.Count();

        var items = query
            .Skip(skip)
            .Take(take)
            .Select(x => new KnowledgeReviewItemViewModel
            {
                Id = x.Id,
                ItemType = x.ItemType,
                Status = x.Status,
                SubjectType = x.SubjectType,
                SubjectStableId = x.SubjectStableId,
                Summary = x.Summary,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt,
                ReviewedAt = x.ReviewedAt
            })
            .ToList();

        return View(new KnowledgeReviewQueueViewModel
        {
            Items = items,
            Page = page,
            PageSize = take,
            TotalCount = total
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateReviewStatus(Guid id, KnowledgeReviewQueueState status, string? note, CancellationToken cancellationToken)
    {
        var item = await _context.KnowledgeReviewItems.FindAsync(new object[] { id }, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        item.Status = status;
        item.ReviewedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(note))
        {
            item.Notes = string.IsNullOrWhiteSpace(item.Notes) ? note : $"{item.Notes}\n{note}";
        }

        await _context.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(ReviewQueue));
    }

    [HttpGet]
    public async Task<IActionResult> Record(string stableId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(stableId))
        {
            return NotFound();
        }

        var record = await _knowledgeQueryService.GetByStableIdAsync(stableId, cancellationToken);
        if (record is null)
        {
            return NotFound();
        }

        var evidence = await _knowledgeEvidenceService.GetEvidenceForRecordAsync(record.Id, cancellationToken);

        var decisions = _context.DecisionHistoryRecords
            .AsQueryable()
            .Where(x => x.SubjectStableId == record.SubjectStableId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new DecisionHistoryRowViewModel
            {
                Id = x.Id,
                DecisionType = x.DecisionType,
                DecisionStatus = x.DecisionStatus,
                SelectedOption = x.SelectedOption,
                ConfidenceScore = x.ConfidenceScore,
                ExplanationSummary = x.ExplanationSummary,
                CreatedAt = x.CreatedAt,
                CompletedAt = x.CompletedAt
            })
            .ToList();

        return View(new KnowledgeRecordDetailViewModel
        {
            Record = record,
            SupportingEvidence = evidence.Where(x => x.IsSupporting).ToList(),
            ContradictingEvidence = evidence.Where(x => x.IsContradicting).ToList(),
            DecisionHistory = decisions,
            RelatedInventoryRecords = _context.Cards.Where(x => x.ChecklistItem != null && x.ChecklistItem.CatalogRecordId == record.SubjectStableId).Select(x => x.CardId).ToList(),
            RelatedScannerSessions = new[] { record.SubjectStableId },
            RelatedPricingResearch = Array.Empty<string>(),
            AuditHistory = _context.KnowledgeAuditRecords.Where(x => x.SubjectStableId == record.SubjectStableId).OrderByDescending(x => x.CreatedAt).ToList()
        });
    }
}

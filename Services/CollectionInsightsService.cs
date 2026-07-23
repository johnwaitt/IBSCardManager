using System.Globalization;
using IBSCardManager.Data;
using IBSCardManager.Entities;
using IBSCardManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IBSCardManager.Services;

public sealed class CollectionInsightsService : ICollectionInsightsService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly IApplicationVersionProvider _applicationVersionProvider;

    public CollectionInsightsService(
        ApplicationDbContext context,
        IMemoryCache cache,
        IApplicationVersionProvider applicationVersionProvider)
    {
        _context = context;
        _cache = cache;
        _applicationVersionProvider = applicationVersionProvider;
    }

    public async Task<CollectionAnalyticsDashboardViewModel> BuildCollectionAnalyticsDashboardAsync(string range, CancellationToken cancellationToken = default)
    {
        var normalizedRange = NormalizeRange(range);
        var cacheKey = $"analytics:dashboard:{normalizedRange}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            await RecalculateAnalyticsAsync("dashboard-refresh", cancellationToken);

            var summaries = await _context.InventoryAnalyticsSummaries
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var cards = await _context.Cards.AsNoTracking().ToListAsync(cancellationToken);
            var historyStart = GetHistoryStart(normalizedRange);
            var snapshotsQuery = _context.CollectionSnapshots.AsNoTracking();
            if (historyStart.HasValue)
            {
                snapshotsQuery = snapshotsQuery.Where(x => x.SnapshotDate >= historyStart.Value);
            }

            var snapshots = await snapshotsQuery
                .OrderBy(x => x.SnapshotDate)
                .ToListAsync(cancellationToken);

            var totalValue = summaries.Sum(x => x.CurrentEstimatedValue);
            var totalCost = summaries.Sum(x => x.CostBasis);
            var unrealizedGainLoss = summaries.Sum(x => x.UnrealizedGainLoss);
            var unrealizedGain = summaries.Where(x => x.UnrealizedGainLoss > 0m).Sum(x => x.UnrealizedGainLoss);
            var unrealizedLoss = summaries.Where(x => x.UnrealizedGainLoss < 0m).Sum(x => Math.Abs(x.UnrealizedGainLoss));
            var gradedCards = cards.Where(IsGraded).Sum(x => x.Quantity);
            var rawCards = cards.Where(x => !IsGraded(x)).Sum(x => x.Quantity);
            var gradedValue = cards.Where(IsGraded).Sum(x => x.MyValue ?? 0m);
            var rawValue = cards.Where(x => !IsGraded(x)).Sum(x => x.MyValue ?? 0m);
            var listedValue = cards.Where(IsListed).Sum(x => x.ListingPrice ?? x.MyValue ?? 0m);
            var unlistedValue = Math.Max(0m, totalValue - listedValue);
            var stalePricing = cards.Count(x => (DateTime.UtcNow - x.ModifiedDate.ToUniversalTime()).TotalDays > 30);
            var needingPricing = cards.Count(x => x.MyValue is null or <= 0);
            var readyToList = cards.Count(x => !IsListed(x) && x.MyValue.HasValue && x.MyValue.Value > 0m);
            var strongGrading = summaries.Count(x => x.GradingScore >= 75m);
            var recentSales = cards.Count(x => string.Equals(x.ListingStatus, "Sold", StringComparison.OrdinalIgnoreCase));
            var recentAcquisitions = cards.Count(x => (DateTime.UtcNow - x.CreatedDate.ToUniversalTime()).TotalDays <= 30);

            var highConfidenceValue = summaries.Where(x => x.PriceConfidence >= 70m).Sum(x => x.CurrentEstimatedValue);
            var lowConfidenceValue = summaries.Where(x => x.PriceConfidence < 70m).Sum(x => x.CurrentEstimatedValue);

            return new CollectionAnalyticsDashboardViewModel
            {
                Range = normalizedRange,
                TotalCollectionValue = totalValue,
                TotalCostBasis = totalCost,
                UnrealizedGainLoss = unrealizedGainLoss,
                UnrealizedGain = unrealizedGain,
                UnrealizedLoss = unrealizedLoss,
                RealizedProfit = 0m,
                TotalCards = cards.Sum(x => x.Quantity),
                UniqueCards = cards.Count,
                RawCards = rawCards,
                GradedCards = gradedCards,
                RawValue = rawValue,
                GradedValue = gradedValue,
                ListedValue = listedValue,
                UnlistedValue = unlistedValue,
                HighConfidenceValue = highConfidenceValue,
                LowConfidenceValue = lowConfidenceValue,
                CardsNeedingPricing = needingPricing,
                CardsWithStalePricing = stalePricing,
                CardsReadyToList = readyToList,
                StrongGradingCandidates = strongGrading,
                RecentSales = recentSales,
                RecentAcquisitions = recentAcquisitions,
                LastCalculatedAt = DateTimeOffset.UtcNow,
                History = snapshots.Select(snapshot => new CollectionSnapshotPointViewModel
                {
                    SnapshotDate = snapshot.SnapshotDate,
                    TotalEstimatedValue = snapshot.TotalEstimatedValue,
                    TotalCostBasis = snapshot.TotalCostBasis,
                    UnrealizedGainLoss = snapshot.TotalEstimatedValue - snapshot.TotalCostBasis,
                    RealizedProfit = snapshot.RealizedProfit,
                    RawValue = snapshot.RawValue,
                    GradedValue = snapshot.GradedValue
                }).ToList()
            };
        }) ?? new CollectionAnalyticsDashboardViewModel();
    }

    public async Task<RecommendationCenterViewModel> BuildRecommendationCenterAsync(CancellationToken cancellationToken = default)
    {
        await RecalculateAnalyticsAsync("recommendation-center", cancellationToken);

        var grade = await _context.RecommendationRecords.AsNoTracking()
            .Where(x => x.RecommendationType == "Grade" && !x.Dismissed)
            .OrderByDescending(x => x.Score)
            .Take(20)
            .Join(_context.Cards.AsNoTracking(), x => x.InventoryCardId, c => c.CardId, (record, card) => ToScoreRow(record, card))
            .ToListAsync(cancellationToken);

        var sell = await _context.RecommendationRecords.AsNoTracking()
            .Where(x => x.RecommendationType == "Sell" && !x.Dismissed)
            .OrderByDescending(x => x.Score)
            .Take(20)
            .Join(_context.Cards.AsNoTracking(), x => x.InventoryCardId, c => c.CardId, (record, card) => ToScoreRow(record, card))
            .ToListAsync(cancellationToken);

        var hold = await _context.RecommendationRecords.AsNoTracking()
            .Where(x => x.RecommendationType == "Hold" && !x.Dismissed)
            .OrderByDescending(x => x.Score)
            .Take(20)
            .Join(_context.Cards.AsNoTracking(), x => x.InventoryCardId, c => c.CardId, (record, card) => ToScoreRow(record, card))
            .ToListAsync(cancellationToken);

        return new RecommendationCenterViewModel
        {
            GradeThese = grade,
            SellThese = sell,
            HoldThese = hold,
            FixTheseRecords = await GetDataQualityIssuesAsync(cancellationToken),
            ListTheseDuplicates = await GetDuplicateAnalyticsAsync(cancellationToken),
            LastCalculatedAt = DateTimeOffset.UtcNow
        };
    }

    public async Task<IReadOnlyList<AnalyticsCardScoreViewModel>> GetTopReportAsync(string reportCode, int take = 50, CancellationToken cancellationToken = default)
    {
        await RecalculateAnalyticsAsync("top-reports", cancellationToken);

        var query = _context.InventoryAnalyticsSummaries.AsNoTracking()
            .Join(_context.Cards.AsNoTracking(), summary => summary.InventoryCardId, card => card.CardId, (summary, card) => new { summary, card });

        query = reportCode switch
        {
            "mostvaluable" => query.OrderByDescending(x => x.summary.CurrentEstimatedValue),
            "highestgains" => query.OrderByDescending(x => x.summary.UnrealizedGainLoss),
            "largestlosses" => query.OrderBy(x => x.summary.UnrealizedGainLoss),
            "highestroi" => query.OrderByDescending(x => x.summary.ROI),
            "lowestroi" => query.OrderBy(x => x.summary.ROI),
            "graded" => query.Where(x => IsGraded(x.card)).OrderByDescending(x => x.summary.CurrentEstimatedValue),
            "raw" => query.Where(x => !IsGraded(x.card)).OrderByDescending(x => x.summary.CurrentEstimatedValue),
            _ => query.OrderByDescending(x => x.summary.CurrentEstimatedValue)
        };

        var rows = await query.Take(Math.Clamp(take, 1, 200)).ToListAsync(cancellationToken);

        return rows.Select(x => new AnalyticsCardScoreViewModel
        {
            CardId = x.card.CardId,
            Title = x.card.Subject,
            SetName = x.card.Set,
            CardNumber = x.card.CardNumber,
            ImageUrl = x.card.FrontImagePath,
            CurrentEstimatedValue = x.summary.CurrentEstimatedValue,
            CostBasis = x.summary.CostBasis,
            UnrealizedGainLoss = x.summary.UnrealizedGainLoss,
            RoiPercent = x.summary.ROI * 100m,
            Score = PickScore(reportCode, x.summary),
            Confidence = x.summary.PriceConfidence,
            Recommendation = x.summary.Recommendation,
            Reason = x.summary.RecommendationReason,
            Risks = x.summary.PriceConfidence < 50m ? "Low price confidence" : "Market movement risk"
        }).ToList();
    }

    public async Task<IReadOnlyList<CollectionConcentrationRowViewModel>> GetConcentrationAsync(string dimension, CancellationToken cancellationToken = default)
    {
        var cards = await _context.Cards.AsNoTracking().ToListAsync(cancellationToken);
        var totalValue = cards.Sum(x => x.MyValue ?? 0m);

        IEnumerable<IGrouping<string, Card>> groups = dimension.ToLowerInvariant() switch
        {
            "player" => cards.GroupBy(x => x.Subject ?? "Unknown"),
            "team" => cards.GroupBy(x => x.Team ?? "Unknown"),
            "product" => cards.GroupBy(x => x.Set ?? "Unknown"),
            "year" => cards.GroupBy(x => x.Year?.ToString(CultureInfo.InvariantCulture) ?? "Unknown"),
            _ => cards.GroupBy(x => x.Subject ?? "Unknown")
        };

        return groups
            .Select(group => new CollectionConcentrationRowViewModel
            {
                Dimension = dimension,
                Key = group.Key,
                CardCount = group.Sum(x => x.Quantity),
                TotalValue = group.Sum(x => x.MyValue ?? 0m),
                CostBasis = group.Sum(x => x.MyCost ?? 0m),
                UnrealizedGainLoss = group.Sum(x => (x.MyValue ?? 0m) - (x.MyCost ?? 0m)),
                PercentageOfCollectionValue = totalValue <= 0m ? 0m : Math.Round((group.Sum(x => x.MyValue ?? 0m) / totalValue) * 100m, 2)
            })
            .OrderByDescending(x => x.TotalValue)
            .Take(200)
            .ToList();
    }

    public async Task<IReadOnlyList<DuplicateAnalyticsRowViewModel>> GetDuplicateAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        var cards = await _context.Cards.AsNoTracking().ToListAsync(cancellationToken);

        return cards
            .GroupBy(x => $"{x.ProductId}|{x.ChecklistItemId}|{x.CardNumber}|{x.Subject}")
            .Where(group => group.Sum(x => x.Quantity) > 1)
            .Select(group =>
            {
                var graded = group.Where(IsGraded).Sum(x => x.Quantity);
                var raw = group.Where(x => !IsGraded(x)).Sum(x => x.Quantity);
                return new DuplicateAnalyticsRowViewModel
                {
                    GroupKey = group.Key,
                    Copies = group.Sum(x => x.Quantity),
                    RawCopies = raw,
                    GradedCopies = graded,
                    Recommendation = graded > 0 && raw > 0
                        ? "Keep strongest graded copy and review raw duplicates"
                        : "List duplicate copies after user approval"
                };
            })
            .OrderByDescending(x => x.Copies)
            .Take(200)
            .ToList();
    }

    public async Task<IReadOnlyList<DataQualityIssueViewModel>> GetDataQualityIssuesAsync(CancellationToken cancellationToken = default)
    {
        var cards = await _context.Cards.AsNoTracking().ToListAsync(cancellationToken);

        return cards
            .Select(card =>
            {
                var missing = new List<string>();
                if (card.ProductId == null) missing.Add("Product ID");
                if (string.IsNullOrWhiteSpace(card.CardNumber)) missing.Add("Card Number");
                if (string.IsNullOrWhiteSpace(card.Subject)) missing.Add("Player");
                if (string.IsNullOrWhiteSpace(card.FrontImagePath)) missing.Add("Front image");
                if (!card.MyCost.HasValue) missing.Add("Cost basis");
                if (!card.MyValue.HasValue) missing.Add("Current value");

                var score = Math.Max(0m, 100m - (missing.Count * 12.5m));
                var classification = score switch
                {
                    >= 90m => "Complete",
                    >= 70m => "Mostly complete",
                    >= 40m => "Needs review",
                    _ => "Poor data quality"
                };

                return new DataQualityIssueViewModel
                {
                    CardId = card.CardId,
                    CardIdentity = $"{card.Subject} {card.Set} #{card.CardNumber}".Trim(),
                    Score = score,
                    Classification = classification,
                    MissingFields = missing
                };
            })
            .Where(x => x.Score < 90m)
            .OrderBy(x => x.Score)
            .Take(200)
            .ToList();
    }

    public async Task<SnapshotCreationResult> CreateSnapshotAsync(string trigger, CancellationToken cancellationToken = default)
    {
        var totalCards = await _context.Cards.AsNoTracking().CountAsync(cancellationToken);
        var totalQuantity = await _context.Cards.AsNoTracking().SumAsync(x => (int?)x.Quantity, cancellationToken) ?? 0;
        var totalValue = await _context.Cards.AsNoTracking().SumAsync(x => x.MyValue ?? 0m, cancellationToken);
        var totalCost = await _context.Cards.AsNoTracking().SumAsync(x => x.MyCost ?? 0m, cancellationToken);
        var gradedValue = await _context.Cards.AsNoTracking().Where(x => !string.IsNullOrWhiteSpace(x.GradeIssuer) || !string.IsNullOrWhiteSpace(x.Grade)).SumAsync(x => x.MyValue ?? 0m, cancellationToken);
        var rawValue = await _context.Cards.AsNoTracking().Where(x => string.IsNullOrWhiteSpace(x.GradeIssuer) && string.IsNullOrWhiteSpace(x.Grade)).SumAsync(x => x.MyValue ?? 0m, cancellationToken);

        var snapshotDate = DateTime.UtcNow;
        var previous = await _context.CollectionSnapshots
            .AsNoTracking()
            .OrderByDescending(x => x.SnapshotDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (previous != null &&
            previous.TotalCards == totalCards &&
            previous.TotalQuantity == totalQuantity &&
            previous.TotalEstimatedValue == totalValue &&
            previous.TotalCostBasis == totalCost &&
            previous.GradedValue == gradedValue &&
            previous.RawValue == rawValue)
        {
            return new SnapshotCreationResult { Created = false, Reason = "Snapshot unchanged; duplicate prevented." };
        }

        var schemaVersion = _context.Database.GetAppliedMigrations().LastOrDefault();

        var snapshot = new CollectionSnapshot
        {
            Id = Guid.NewGuid(),
            SnapshotDate = snapshotDate,
            TotalCards = totalCards,
            UniqueCards = totalCards,
            TotalQuantity = totalQuantity,
            TotalEstimatedValue = totalValue,
            TotalCostBasis = totalCost,
            UnrealizedGain = Math.Max(0m, totalValue - totalCost),
            UnrealizedLoss = Math.Max(0m, totalCost - totalValue),
            RealizedProfit = 0m,
            ListedValue = await _context.Cards.AsNoTracking().Where(x => !string.IsNullOrWhiteSpace(x.ListingStatus) && !string.Equals(x.ListingStatus, "Not Listed")).SumAsync(x => x.ListingPrice ?? x.MyValue ?? 0m, cancellationToken),
            SoldValue = 0m,
            GradedValue = gradedValue,
            RawValue = rawValue,
            HighConfidenceValue = await _context.InventoryAnalyticsSummaries.AsNoTracking().Where(x => x.PriceConfidence >= 70m).SumAsync(x => x.CurrentEstimatedValue, cancellationToken),
            LowConfidenceValue = await _context.InventoryAnalyticsSummaries.AsNoTracking().Where(x => x.PriceConfidence < 70m).SumAsync(x => x.CurrentEstimatedValue, cancellationToken),
            ApplicationVersion = _applicationVersionProvider.ApplicationVersion,
            SchemaVersion = schemaVersion ?? "Unknown"
        };

        _context.CollectionSnapshots.Add(snapshot);
        await _context.SaveChangesAsync(cancellationToken);

        return new SnapshotCreationResult { Created = true, Reason = $"Snapshot created from trigger '{trigger}'." };
    }

    public async Task RecalculateAnalyticsAsync(string trigger, CancellationToken cancellationToken = default)
    {
        var cards = await _context.Cards.AsNoTracking().ToListAsync(cancellationToken);

        var existing = await _context.InventoryAnalyticsSummaries.ToDictionaryAsync(x => x.InventoryCardId, cancellationToken);
        var now = DateTime.UtcNow;

        foreach (var card in cards)
        {
            var currentValue = card.MyValue ?? 0m;
            var costBasis = card.MyCost ?? 0m;
            var gainLoss = currentValue - costBasis;
            var roi = costBasis > 0m ? Math.Round(gainLoss / costBasis, 4) : 0m;
            var priceConfidence = ResolvePriceConfidence(card);
            var gradingRoi = CalculateGradingRoi(card, currentValue, costBasis, priceConfidence);
            var gradingScore = ResolveGradingScore(card, currentValue, costBasis, priceConfidence, gradingRoi.ExpectedNetProfit, gradingRoi.ExpectedRoiPercent);
            var liquidity = ResolveLiquidityScore(card);
            var demand = ResolveDemandScore(card);
            var sellScore = ResolveSellScore(card, gainLoss, priceConfidence, liquidity);
            var holdScore = ResolveHoldScore(card, gainLoss, priceConfidence, demand);
            var recommendation = ResolveRecommendation(gradingScore, sellScore, holdScore, priceConfidence);
            var freshness = ResolveFreshness(card);

            if (!existing.TryGetValue(card.CardId, out var summary))
            {
                summary = new InventoryAnalyticsSummary
                {
                    InventoryCardId = card.CardId
                };
                _context.InventoryAnalyticsSummaries.Add(summary);
                existing[card.CardId] = summary;
            }

            summary.CurrentEstimatedValue = currentValue;
            summary.CostBasis = costBasis;
            summary.UnrealizedGainLoss = gainLoss;
            summary.ROI = roi;
            summary.PriceConfidence = priceConfidence;
            summary.PriceFreshness = freshness;
            summary.GradingScore = gradingScore;
            summary.SellScore = sellScore;
            summary.HoldScore = holdScore;
            summary.LiquidityScore = liquidity;
            summary.DemandScore = demand;
            summary.Recommendation = recommendation;
            summary.RecommendationReason = BuildReason(gradingScore, sellScore, holdScore, priceConfidence);
            summary.LastCalculatedAt = now;
        }

        var staleIds = existing.Keys.Except(cards.Select(c => c.CardId)).ToList();
        foreach (var staleId in staleIds)
        {
            _context.InventoryAnalyticsSummaries.Remove(existing[staleId]);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await RebuildRecommendationRecordsAsync(now, cancellationToken);

        if (!trigger.StartsWith("snapshot-", StringComparison.OrdinalIgnoreCase))
        {
            await CreateSnapshotAsync($"auto-{trigger}", cancellationToken);
        }

        InvalidateCaches();
    }

    private async Task RebuildRecommendationRecordsAsync(DateTime now, CancellationToken cancellationToken)
    {
        var summaries = await _context.InventoryAnalyticsSummaries.AsNoTracking().ToListAsync(cancellationToken);
        var cards = await _context.Cards.AsNoTracking().ToDictionaryAsync(x => x.CardId, cancellationToken);

        var existingStates = await _context.RecommendationRecords
            .AsNoTracking()
            .GroupBy(x => new { x.InventoryCardId, x.RecommendationType })
            .Select(group => group.OrderByDescending(x => x.GeneratedAt).First())
            .ToListAsync(cancellationToken);

        _context.RecommendationRecords.RemoveRange(_context.RecommendationRecords);

        foreach (var summary in summaries)
        {
            if (!cards.TryGetValue(summary.InventoryCardId, out var card))
            {
                continue;
            }

            var gradeState = existingStates.FirstOrDefault(x => x.InventoryCardId == summary.InventoryCardId && x.RecommendationType == "Grade");
            var sellState = existingStates.FirstOrDefault(x => x.InventoryCardId == summary.InventoryCardId && x.RecommendationType == "Sell");
            var holdState = existingStates.FirstOrDefault(x => x.InventoryCardId == summary.InventoryCardId && x.RecommendationType == "Hold");

            _context.RecommendationRecords.Add(BuildRecord(card, summary, "Grade", summary.GradingScore, ResolveGradeSummary(summary), gradeState, now, _applicationVersionProvider.ApplicationVersion, alternative: "Sell raw or hold", assumption: "Expected grade probabilities use configurable baseline", missingData: summary.PriceConfidence < 50m));
            _context.RecommendationRecords.Add(BuildRecord(card, summary, "Sell", summary.SellScore, ResolveSellSummary(summary), sellState, now, _applicationVersionProvider.ApplicationVersion, alternative: "Hold for now", assumption: "Liquidity inferred from local listing/readiness signals", missingData: summary.PriceConfidence < 50m));
            _context.RecommendationRecords.Add(BuildRecord(card, summary, "Hold", summary.HoldScore, ResolveHoldSummary(summary), holdState, now, _applicationVersionProvider.ApplicationVersion, alternative: "Sell now", assumption: "No speculative upside claims beyond observed confidence", missingData: summary.PriceConfidence < 50m));
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static RecommendationRecord BuildRecord(Card card, InventoryAnalyticsSummary summary, string type, decimal score, string summaryText, RecommendationRecord? previous, DateTime now, string version, string alternative, string assumption, bool missingData)
    {
        var reasons = new List<string>
        {
            $"Inputs: value={summary.CurrentEstimatedValue.ToString("C0", CultureInfo.CurrentCulture)}, cost={summary.CostBasis.ToString("C0", CultureInfo.CurrentCulture)}, gain/loss={summary.UnrealizedGainLoss.ToString("C0", CultureInfo.CurrentCulture)}",
            $"Rules applied: grade={summary.GradingScore:N1}, sell={summary.SellScore:N1}, hold={summary.HoldScore:N1}",
            $"Confidence: {summary.PriceConfidence:N0}%"
        };

        if (missingData)
        {
            reasons.Add("Missing data: low-confidence valuation and/or sparse pricing comps.");
        }

        reasons.Add($"Assumption: {assumption}");
        reasons.Add($"Alternative not selected: {alternative}");

        var risks = summary.PriceConfidence < 60m
            ? "Low confidence valuation may shift with additional comps."
            : "Market demand and listing competition can change quickly.";

        var requiredAction = type switch
        {
            "Grade" => "Review condition/photos, grading fees, and break-even threshold before submission.",
            "Sell" => "Review active listings and create/update listing draft.",
            _ => "Set review date and monitor confidence, trend, and liquidity changes."
        };

        var confidence = Math.Min(99m, Math.Max(10m, summary.PriceConfidence));

        return new RecommendationRecord
        {
            Id = Guid.NewGuid(),
            InventoryCardId = card.CardId,
            RecommendationType = type,
            Score = Math.Round(score, 2),
            Confidence = Math.Round(confidence, 2),
            Summary = summaryText,
            Reasons = string.Join(" | ", reasons),
            Risks = risks,
            RequiredActions = requiredAction,
            GeneratedAt = now,
            Accepted = previous?.Accepted ?? false,
            Dismissed = previous?.Dismissed ?? false,
            SnoozedUntil = previous?.SnoozedUntil,
            UserNotes = previous?.UserNotes,
            ModelVersion = version,
            RuleVersion = version
        };
    }

    private static string ResolveGradeSummary(InventoryAnalyticsSummary summary)
    {
        if (summary.GradingScore >= 80m) return "Strong grading candidate";
        if (summary.GradingScore >= 65m) return "Good grading candidate";
        if (summary.GradingScore >= 50m) return "Borderline grading candidate";
        if (summary.PriceConfidence < 40m) return "Insufficient data for grading recommendation";
        return "Sell raw";
    }

    private static string ResolveSellSummary(InventoryAnalyticsSummary summary)
    {
        if (summary.PriceConfidence < 40m) return "Research first";
        if (summary.SellScore >= 80m) return "Sell now";
        if (summary.SellScore >= 60m) return "List soon";
        return "Hold for now";
    }

    private static string ResolveHoldSummary(InventoryAnalyticsSummary summary)
    {
        if (summary.PriceConfidence < 40m) return "Insufficient data";
        if (summary.HoldScore >= 75m) return "Hold for now";
        if (summary.HoldScore >= 55m) return "Review in 30 days";
        return "Re-evaluate against sell options";
    }

    private static decimal PickScore(string reportCode, InventoryAnalyticsSummary summary) => reportCode switch
    {
        "highestroi" or "lowestroi" => summary.ROI * 100m,
        "highestgains" or "largestlosses" => summary.UnrealizedGainLoss,
        "graded" or "raw" or "mostvaluable" => summary.CurrentEstimatedValue,
        _ => summary.CurrentEstimatedValue
    };

    private static string NormalizeRange(string range)
    {
        return range?.Trim().ToLowerInvariant() switch
        {
            "7d" => "7d",
            "30d" => "30d",
            "90d" => "90d",
            "1y" => "1y",
            _ => "all"
        };
    }

    private static DateTime? GetHistoryStart(string range)
    {
        var now = DateTime.UtcNow;
        return range switch
        {
            "7d" => now.AddDays(-7),
            "30d" => now.AddDays(-30),
            "90d" => now.AddDays(-90),
            "1y" => now.AddYears(-1),
            _ => null
        };
    }

    private static bool IsGraded(Card card) => !string.IsNullOrWhiteSpace(card.GradeIssuer) || !string.IsNullOrWhiteSpace(card.Grade);

    private static bool IsListed(Card card)
    {
        if (string.IsNullOrWhiteSpace(card.ListingStatus)) return false;
        return !string.Equals(card.ListingStatus, "Not Listed", StringComparison.OrdinalIgnoreCase);
    }

    private static decimal ResolvePriceConfidence(Card card)
    {
        var confidence = 35m;
        if (card.MyValue.HasValue) confidence += 30m;
        if (card.ModifiedDate > DateTime.UtcNow.AddDays(-14)) confidence += 15m;
        if (!string.IsNullOrWhiteSpace(card.Set)) confidence += 5m;
        if (!string.IsNullOrWhiteSpace(card.CardNumber)) confidence += 5m;
        if (card.ProductId.HasValue && card.ChecklistItemId.HasValue) confidence += 10m;
        return Math.Min(95m, confidence);
    }

    private static string ResolveFreshness(Card card)
    {
        var ageDays = (DateTime.UtcNow - card.ModifiedDate.ToUniversalTime()).TotalDays;
        return ageDays switch
        {
            <= 7 => "Fresh",
            <= 30 => "Moderate",
            <= 90 => "Stale",
            _ => "Very stale"
        };
    }

    private static decimal ResolveLiquidityScore(Card card)
    {
        var score = 25m;
        if (IsListed(card)) score += 20m;
        if (card.IsRookie) score += 15m;
        if (card.IsAutograph) score += 15m;
        if (card.Quantity > 1) score += 10m;
        if (card.MyValue.GetValueOrDefault() >= 100m) score += 10m;
        return Math.Min(95m, score);
    }

    private static decimal ResolveDemandScore(Card card)
    {
        var score = 30m;
        if (card.IsRookie) score += 20m;
        if (card.IsAutograph) score += 15m;
        if (card.IsRefractor) score += 10m;
        if (!string.IsNullOrWhiteSpace(card.Team)) score += 5m;
        if (!string.IsNullOrWhiteSpace(card.Subject)) score += 10m;
        return Math.Min(95m, score);
    }

    private static decimal ResolveGradingScore(Card card, decimal currentValue, decimal costBasis, decimal priceConfidence, decimal expectedNetProfit, decimal expectedRoiPercent)
    {
        var score = 20m;
        if (!IsGraded(card)) score += 20m;
        if (card.PsaEstimate.HasValue && card.PsaEstimate.Value > currentValue) score += 15m;
        if (expectedNetProfit > 0m) score += 15m;
        if (expectedRoiPercent >= 20m) score += 10m;
        if (card.IsRookie) score += 8m;
        if (card.IsAutograph) score += 8m;
        if (!string.IsNullOrWhiteSpace(card.Serial)) score += 4m;
        if (currentValue >= 50m) score += 5m;
        if (priceConfidence >= 70m) score += 10m;
        if (costBasis > 0 && currentValue < costBasis) score -= 5m;
        return Math.Clamp(score, 0m, 100m);
    }

    private static decimal ResolveSellScore(Card card, decimal gainLoss, decimal priceConfidence, decimal liquidity)
    {
        var score = 25m;
        if (gainLoss > 0m) score += 20m;
        if (IsListed(card)) score += 15m;
        if (card.Quantity > 1) score += 15m;
        if (priceConfidence >= 70m) score += 10m;
        if (liquidity >= 60m) score += 10m;
        if (card.ModifiedDate < DateTime.UtcNow.AddDays(-30)) score += 5m;
        return Math.Clamp(score, 0m, 100m);
    }

    private static decimal ResolveHoldScore(Card card, decimal gainLoss, decimal priceConfidence, decimal demand)
    {
        var score = 25m;
        if (gainLoss < 0m) score += 10m;
        if (card.IsRookie) score += 15m;
        if (card.IsAutograph) score += 10m;
        if (!IsListed(card)) score += 10m;
        if (priceConfidence >= 60m) score += 10m;
        if (demand >= 60m) score += 10m;
        return Math.Clamp(score, 0m, 100m);
    }

    private static string ResolveRecommendation(decimal gradingScore, decimal sellScore, decimal holdScore, decimal priceConfidence)
    {
        if (priceConfidence < 40m) return "Insufficient data";
        if (gradingScore >= sellScore && gradingScore >= holdScore && gradingScore >= 70m) return "Strong grading candidate";
        if (sellScore >= holdScore && sellScore >= 70m) return "Sell now";
        if (holdScore >= 65m) return "Hold for now";
        if (sellScore >= 55m) return "List soon";
        if (gradingScore >= 55m) return "Good grading candidate";
        return "Research first";
    }

    private static string BuildReason(decimal gradingScore, decimal sellScore, decimal holdScore, decimal priceConfidence)
    {
        return $"Scores: grade={gradingScore:N1}, sell={sellScore:N1}, hold={holdScore:N1}; confidence={priceConfidence:N1}%. Recommendation chosen by highest score with confidence guardrails.";
    }

    private void InvalidateCaches()
    {
        _cache.Remove("analytics:dashboard:7d");
        _cache.Remove("analytics:dashboard:30d");
        _cache.Remove("analytics:dashboard:90d");
        _cache.Remove("analytics:dashboard:1y");
        _cache.Remove("analytics:dashboard:all");
    }

    private static AnalyticsCardScoreViewModel ToScoreRow(RecommendationRecord record, Card card)
    {
        var cost = card.MyCost ?? 0m;
        var value = card.MyValue ?? 0m;
        var gain = value - cost;
        var roi = cost > 0m ? (gain / cost) * 100m : 0m;
        var grading = CalculateGradingRoi(card, value, cost, Math.Max(30m, record.Confidence));

        return new AnalyticsCardScoreViewModel
        {
            CardId = card.CardId,
            Title = card.Subject,
            SetName = card.Set,
            CardNumber = card.CardNumber,
            ImageUrl = card.FrontImagePath,
            CurrentEstimatedValue = value,
            CostBasis = cost,
            UnrealizedGainLoss = gain,
            RoiPercent = roi,
            Score = record.Score,
            Confidence = record.Confidence,
            Recommendation = record.Summary,
            Reason = record.Reasons,
            Risks = record.Risks,
            ExpectedGradedValue = grading.ExpectedGradedValue,
            ExpectedNetProfit = grading.ExpectedNetProfit,
            ExpectedRoiPercent = grading.ExpectedRoiPercent,
            BreakEvenGrade = grading.BreakEvenGrade,
            MinTopGradeProbability = grading.MinTopGradeProbability,
            BestGrader = grading.BestGrader
        };
    }

    private sealed record GradingRoiResult(
        decimal ExpectedGradedValue,
        decimal ExpectedNetProfit,
        decimal ExpectedRoiPercent,
        string BreakEvenGrade,
        decimal MinTopGradeProbability,
        string BestGrader,
        string Recommendation);

    private static GradingRoiResult CalculateGradingRoi(Card card, decimal rawValue, decimal costBasis, decimal priceConfidence)
    {
        var psa10 = card.PsaEstimate ?? rawValue * 1.8m;
        var psa9 = rawValue * 1.35m;
        var bgs95 = rawValue * 1.45m;
        var sgc10 = rawValue * 1.55m;

        var p10 = 0.20m;
        var p9 = 0.45m;
        var pOther = 0.35m;

        if (priceConfidence >= 75m)
        {
            p10 += 0.05m;
            p9 += 0.05m;
            pOther -= 0.10m;
        }

        var gradingFees = 24m;
        var shipping = 5m;
        var insurance = 3m;
        var totalCost = gradingFees + shipping + insurance;

        var expectedPsa = (psa10 * p10) + (psa9 * p9) + (rawValue * pOther);
        var expectedBgs = (bgs95 * (p10 + 0.05m)) + (rawValue * (1m - (p10 + 0.05m)));
        var expectedSgc = (sgc10 * p10) + (rawValue * (1m - p10));

        var options = new[]
        {
            new { Name = "PSA", Expected = expectedPsa },
            new { Name = "BGS", Expected = expectedBgs },
            new { Name = "SGC", Expected = expectedSgc }
        };

        var best = options.OrderByDescending(x => x.Expected).First();
        var expectedNet = best.Expected - rawValue - totalCost;
        var expectedRoi = (rawValue + totalCost) > 0m ? (expectedNet / (rawValue + totalCost)) * 100m : 0m;

        var breakEvenGrade = expectedNet >= 0m ? "PSA 9" : "PSA 10";
        var minTopProb = Math.Clamp(((rawValue + totalCost) - psa9) / Math.Max(1m, psa10 - psa9), 0m, 1m) * 100m;

        var recommendation = expectedNet switch
        {
            >= 25m => "Grade now",
            >= 0m => "Hold",
            _ => "Sell raw"
        };

        return new GradingRoiResult(
            ExpectedGradedValue: Math.Round(best.Expected, 2),
            ExpectedNetProfit: Math.Round(expectedNet, 2),
            ExpectedRoiPercent: Math.Round(expectedRoi, 2),
            BreakEvenGrade: breakEvenGrade,
            MinTopGradeProbability: Math.Round(minTopProb, 2),
            BestGrader: best.Name,
            Recommendation: recommendation);
    }
}

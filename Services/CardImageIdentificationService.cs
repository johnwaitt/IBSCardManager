using IBSCardManager.Data;
using IBSCardManager.Entities;
using IBSCardManager.Models;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Services;

public sealed class CardImageIdentificationService : ICardImageIdentificationService, ICardImageAnalysisService, ICardMetadataExtractionService, ICardCandidateMatchingService
{
    private readonly ApplicationDbContext _context;
    private readonly IOpenAiCardAnalysisService _openAiCardAnalysis;

    public CardImageIdentificationService(ApplicationDbContext context, IOpenAiCardAnalysisService openAiCardAnalysis)
    {
        _context = context;
        _openAiCardAnalysis = openAiCardAnalysis;
    }

    public async Task<ScannerIdentificationResult> AnalyzePairAsync(ScannerIdentificationRequest request, CancellationToken cancellationToken = default)
    {
        var extraction = await ExtractAsync(request, cancellationToken);
        var candidates = await FindCandidatesAsync(extraction, null, cancellationToken);
        extraction.Candidates = candidates;
        extraction.OverallConfidence = candidates.FirstOrDefault()?.Confidence / 100m ?? extraction.OverallConfidence;
        return extraction;
    }

    public Task<ScannerIdentificationResult> AnalyzeAsync(ScannerIdentificationRequest request, CancellationToken cancellationToken = default)
        => AnalyzePairAsync(request, cancellationToken);

    public async Task<ScannerIdentificationResult> ExtractAsync(ScannerIdentificationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FrontPath) && string.IsNullOrWhiteSpace(request.FrontFileName))
        {
            return new ScannerIdentificationResult { Notes = "No front image was supplied." };
        }

        if (!_openAiCardAnalysis.IsConfigured)
        {
            return new ScannerIdentificationResult { Notes = "ChatGPT image analysis is not configured." };
        }

        var response = await _openAiCardAnalysis.AnalyzeAsync(new CardAnalysisRequest
        {
            FrontImagePath = request.FrontPath ?? string.Empty,
            BackImagePath = request.BackPath,
            Hints = new CardAnalysisHints
            {
                PlayerName = request.Hints.Player,
                Team = request.Hints.Team,
                Year = request.Hints.Year,
                Product = request.Hints.Product,
                CardNumber = request.Hints.CardNumber,
                Parallel = request.Hints.Parallel,
                Variation = request.Hints.Variation
            }
        }, cancellationToken);

        var analysis = response.Analysis ?? new CardAnalysisResult();
        var hasBack = !string.IsNullOrWhiteSpace(request.BackPath);

        return new ScannerIdentificationResult
        {
            Player = MapField(analysis.PlayerName, request.Hints.Player),
            Team = MapField(analysis.Team, request.Hints.Team),
            Year = MapField(analysis.Year, request.Hints.Year?.ToString()),
            Manufacturer = MapField(analysis.Manufacturer, request.Hints.Manufacturer),
            Brand = MapField(analysis.Brand, request.Hints.Brand),
            Product = MapField(analysis.Product, request.Hints.Product),
            ProductEdition = MapField(analysis.ProductEdition, null),
            CardNumber = MapField(analysis.CardNumber, request.Hints.CardNumber),
            ChecklistSection = MapField(analysis.ChecklistSection, request.Hints.ChecklistSection),
            Parallel = MapField(analysis.Parallel, request.Hints.Parallel),
            Variation = MapField(analysis.Variation, request.Hints.Variation),
            Rookie = MapField(analysis.Rookie, request.Hints.IsRookie?.ToString()),
            Autograph = MapField(analysis.Autograph, request.Hints.IsAutograph?.ToString()),
            Relic = MapField(analysis.Relic, request.Hints.IsRelic?.ToString()),
            Patch = MapField(analysis.Patch, request.Hints.IsPatch?.ToString()),
            ShortPrint = MapField(analysis.ShortPrint, null),
            SerialNumber = MapField(analysis.SerialNumber, request.Hints.SerialNumber),
            SerialMaximum = MapField(analysis.SerialMaximum, request.Hints.SerialMaximum?.ToString()),
            CopyrightYear = MapField(analysis.PrintedCopyrightYear, request.Hints.Year?.ToString()),
            VisibleText = BuildField(BuildVisibleText(analysis), null, 0.75m, ScannerFieldSources.Both),
            Orientation = BuildField(hasBack ? "Front and back" : "Front only", null, 1m, hasBack ? ScannerFieldSources.Both : ScannerFieldSources.Front),
            SamePhysicalCard = BuildField("True", null, 1m, hasBack ? ScannerFieldSources.Both : ScannerFieldSources.Front),
            OverallConfidence = decimal.Clamp(analysis.Confidence, 0m, 1m),
            UsedCachedAnalysis = response.Cached,
            Warnings = analysis.Warnings.Select(x => x.Message).Where(x => !string.IsNullOrWhiteSpace(x)).ToList(),
            Evidence = analysis.Evidence,
            Notes = string.Join("; ", analysis.Warnings.Select(x => x.Message).Where(x => !string.IsNullOrWhiteSpace(x)))
        };
    }

    public async Task<IReadOnlyList<ScannerCandidateResult>> FindCandidatesAsync(ScannerIdentificationResult extraction, Guid? productId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ChecklistItems
            .AsNoTracking()
            .Include(x => x.Product)
            .ThenInclude(product => product!.Brand)
            .AsQueryable();

        var normalizedCardNumber = NormalizeCardNumber(extraction.CardNumber.Value);
        var hasCardEvidence = !string.IsNullOrWhiteSpace(normalizedCardNumber);
        var hasPlayerEvidence = !string.IsNullOrWhiteSpace(extraction.Player.Value);

        if (productId.HasValue)
        {
            query = query.Where(x => x.ProductId == productId.Value);
        }

        if (hasCardEvidence)
        {
            query = query.Where(x => x.CardNumber.Contains(extraction.CardNumber.Value!));
        }

        if (hasPlayerEvidence && !hasCardEvidence)
        {
            query = query.Where(x => x.Subject.Contains(extraction.Player.Value!));
        }

        if (!hasCardEvidence && !hasPlayerEvidence && !string.IsNullOrWhiteSpace(extraction.Product.Value))
        {
            query = query.Where(x => x.Product != null && x.Product.DisplayName.Contains(extraction.Product.Value));
        }

        if (!string.IsNullOrWhiteSpace(extraction.Team.Value))
        {
            query = query.Where(x => x.Team != null && x.Team.Contains(extraction.Team.Value));
        }

        if (int.TryParse(extraction.Year.Value, out var extractedYear))
        {
            query = query.Where(x => x.Product != null && x.Product.Year == extractedYear);
        }

        var items = await query.Take(80).ToListAsync(cancellationToken);

        return items
            .Select(item => BuildCandidate(extraction, item))
            .OrderByDescending(x => x.Confidence)
            .ThenBy(x => x.Conflicts.Count)
            .ThenBy(x => x.Player)
            .Take(10)
            .ToList();
    }

    private static ScannerCandidateResult BuildCandidate(ScannerIdentificationResult extraction, ChecklistItem item)
    {
        int? serialMaximum = null;
        var serialSource = item.PrintRun ?? item.SerialNumber;
        if (int.TryParse(serialSource, out var parsedSerial)) serialMaximum = parsedSerial;

        var score = Score(extraction, item, serialMaximum);

        return new ScannerCandidateResult
        {
            ChecklistItemId = item.ChecklistItemId,
            ProductId = item.ProductId,
            Player = item.Subject,
            Team = item.Team,
            Year = item.Product?.Year,
            Manufacturer = item.Product?.Brand?.BrandName,
            Brand = item.Product?.Brand?.BrandName,
            Product = item.Product?.DisplayName,
            ChecklistSection = item.Subset,
            CardNumber = item.CardNumber,
            Parallel = item.Parallel,
            Variation = item.Variation,
            IsRookie = item.IsRookie,
            IsAutograph = item.IsAutograph,
            IsRelic = item.IsRelic,
            IsPatch = false,
            SerialMaximum = serialMaximum,
            ReferenceImageUrl = item.StockImageUrl,
            CatalogSource = "Local checklist",
            MatchStatus = MatchStatus(score),
            Confidence = score,
            MatchReasons = BuildReasons(extraction, item, serialMaximum),
            Conflicts = BuildConflicts(extraction, item)
        };
    }

    private static ScannerExtractionField MapField<T>(CardFieldResult<T> field, string? hint)
    {
        var value = field?.Value?.ToString();
        var source = field == null ? ScannerFieldSources.Unknown : MapSource(field.EvidenceSource);
        var confidence = field?.Confidence ?? 0m;
        return BuildField(value, hint, confidence, source);
    }

    private static string BuildVisibleText(CardAnalysisResult analysis)
    {
        var front = analysis.FrontText.Value?.Trim();
        var back = analysis.BackText.Value?.Trim();
        return string.Join("\n\n", new[]
        {
            string.IsNullOrWhiteSpace(front) ? null : $"Front: {front}",
            string.IsNullOrWhiteSpace(back) ? null : $"Back: {back}"
        }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static string MapSource(string? source)
    {
        return source?.ToLowerInvariant() switch
        {
            CardEvidenceSource.Front => ScannerFieldSources.Front,
            CardEvidenceSource.Back => ScannerFieldSources.Back,
            CardEvidenceSource.Both => ScannerFieldSources.Both,
            CardEvidenceSource.UserCorrected => ScannerFieldSources.UserCorrected,
            _ => ScannerFieldSources.Unknown
        };
    }

    private static ScannerExtractionField BuildField(string? value, string? hint, decimal confidence, string source)
    {
        var final = !string.IsNullOrWhiteSpace(value) ? value : hint;
        var effectiveSource = !string.IsNullOrWhiteSpace(value)
            ? source
            : !string.IsNullOrWhiteSpace(hint)
                ? ScannerFieldSources.UserHint
                : ScannerFieldSources.Unknown;

        return new ScannerExtractionField
        {
            Value = final,
            NormalizedValue = final?.Trim().ToLowerInvariant(),
            Confidence = string.IsNullOrWhiteSpace(final) ? 0m : Math.Min(1m, confidence),
            Source = effectiveSource
        };
    }

    private static decimal Score(ScannerIdentificationResult extraction, ChecklistItem item, int? serialMaximum)
    {
        var score = 0m;

        if (!string.IsNullOrWhiteSpace(extraction.CardNumber.Value) && string.Equals(NormalizeCardNumber(extraction.CardNumber.Value), NormalizeCardNumber(item.CardNumber), StringComparison.OrdinalIgnoreCase)) score += 30m;
        if (!string.IsNullOrWhiteSpace(extraction.Player.Value) && string.Equals(extraction.Player.Value, item.Subject, StringComparison.OrdinalIgnoreCase)) score += 25m;
        if (!string.IsNullOrWhiteSpace(extraction.Product.Value) && string.Equals(extraction.Product.Value, item.Product?.DisplayName, StringComparison.OrdinalIgnoreCase)) score += 20m;
        if (int.TryParse(extraction.Year.Value, out var extractedYear) && item.Product?.Year == extractedYear) score += 10m;
        if (!string.IsNullOrWhiteSpace(extraction.Team.Value) && string.Equals(extraction.Team.Value, item.Team, StringComparison.OrdinalIgnoreCase)) score += 5m;
        if (!string.IsNullOrWhiteSpace(extraction.Parallel.Value) && string.Equals(extraction.Parallel.Value, item.Parallel, StringComparison.OrdinalIgnoreCase)) score += 5m;
        if (!string.IsNullOrWhiteSpace(extraction.Variation.Value) && string.Equals(extraction.Variation.Value, item.Variation, StringComparison.OrdinalIgnoreCase)) score += 5m;
        if (int.TryParse(extraction.SerialMaximum.Value, out var extractedSerialMax) && serialMaximum.HasValue && serialMaximum.Value == extractedSerialMax) score += 5m;

        if (!string.IsNullOrWhiteSpace(extraction.Player.Value) && !string.Equals(extraction.Player.Value, item.Subject, StringComparison.OrdinalIgnoreCase)) score -= 45m;
        if (!string.IsNullOrWhiteSpace(extraction.CardNumber.Value) && !string.Equals(NormalizeCardNumber(extraction.CardNumber.Value), NormalizeCardNumber(item.CardNumber), StringComparison.OrdinalIgnoreCase)) score -= 45m;
        if (!string.IsNullOrWhiteSpace(extraction.Product.Value) && !string.Equals(extraction.Product.Value, item.Product?.DisplayName, StringComparison.OrdinalIgnoreCase)) score -= 30m;
        if (int.TryParse(extraction.Year.Value, out extractedYear) && item.Product?.Year != extractedYear) score -= 12m;
        if (!string.IsNullOrWhiteSpace(extraction.Team.Value) && !string.Equals(extraction.Team.Value, item.Team, StringComparison.OrdinalIgnoreCase)) score -= 8m;
        if (!string.IsNullOrWhiteSpace(extraction.Parallel.Value) && !string.Equals(extraction.Parallel.Value, item.Parallel, StringComparison.OrdinalIgnoreCase)) score -= 8m;
        if (!string.IsNullOrWhiteSpace(extraction.Variation.Value) && !string.Equals(extraction.Variation.Value, item.Variation, StringComparison.OrdinalIgnoreCase)) score -= 8m;

        var hasPlayerEvidence = !string.IsNullOrWhiteSpace(extraction.Player.Value);
        var hasCardEvidence = !string.IsNullOrWhiteSpace(extraction.CardNumber.Value);
        var exactProduct = !string.IsNullOrWhiteSpace(extraction.Product.Value) && string.Equals(extraction.Product.Value, item.Product?.DisplayName, StringComparison.OrdinalIgnoreCase);
        var onlyProductMatch = exactProduct && !hasPlayerEvidence && !hasCardEvidence;

        if (onlyProductMatch) score = Math.Min(score, 64m);

        return decimal.Clamp(score, 0m, 100m);
    }

    private static string MatchStatus(decimal score)
    {
        if (score >= 95m) return "Very Strong Match";
        if (score >= 85m) return "Strong Match";
        if (score >= 70m) return "Review Required";
        return "Weak Match";
    }

    private static IReadOnlyList<string> BuildReasons(ScannerIdentificationResult extraction, ChecklistItem item, int? serialMaximum)
    {
        var reasons = new List<string>();
        if (string.Equals(extraction.Player.Value, item.Subject, StringComparison.OrdinalIgnoreCase)) reasons.Add("Exact player");
        if (string.Equals(NormalizeCardNumber(extraction.CardNumber.Value), NormalizeCardNumber(item.CardNumber), StringComparison.OrdinalIgnoreCase)) reasons.Add("Exact card number");
        if (string.Equals(extraction.Product.Value, item.Product?.DisplayName, StringComparison.OrdinalIgnoreCase)) reasons.Add("Exact set");
        if (int.TryParse(extraction.Year.Value, out var extractedYear) && item.Product?.Year == extractedYear) reasons.Add("Exact year");
        if (string.Equals(extraction.Team.Value, item.Team, StringComparison.OrdinalIgnoreCase)) reasons.Add("Team match");
        if (string.Equals(extraction.Parallel.Value, item.Parallel, StringComparison.OrdinalIgnoreCase)) reasons.Add("Parallel match");
        if (string.Equals(extraction.Variation.Value, item.Variation, StringComparison.OrdinalIgnoreCase)) reasons.Add("Variation match");
        if (int.TryParse(extraction.SerialMaximum.Value, out var extractedSerialMax) && serialMaximum.HasValue && serialMaximum.Value == extractedSerialMax) reasons.Add("Serial maximum match");
        return reasons;
    }

    private static IReadOnlyList<string> BuildConflicts(ScannerIdentificationResult extraction, ChecklistItem item)
    {
        var conflicts = new List<string>();
        if (!string.IsNullOrWhiteSpace(extraction.Player.Value) && !string.Equals(extraction.Player.Value, item.Subject, StringComparison.OrdinalIgnoreCase)) conflicts.Add("Player differs");
        if (!string.IsNullOrWhiteSpace(extraction.CardNumber.Value) && !string.Equals(NormalizeCardNumber(extraction.CardNumber.Value), NormalizeCardNumber(item.CardNumber), StringComparison.OrdinalIgnoreCase)) conflicts.Add("Card number differs");
        if (!string.IsNullOrWhiteSpace(extraction.Product.Value) && !string.Equals(extraction.Product.Value, item.Product?.DisplayName, StringComparison.OrdinalIgnoreCase)) conflicts.Add("Set differs");
        if (int.TryParse(extraction.Year.Value, out var extractedYear) && item.Product?.Year != extractedYear) conflicts.Add("Year uncertain");
        if (!string.IsNullOrWhiteSpace(extraction.Parallel.Value) && !string.Equals(extraction.Parallel.Value, item.Parallel, StringComparison.OrdinalIgnoreCase)) conflicts.Add("Parallel not confirmed");
        if (!string.IsNullOrWhiteSpace(extraction.Variation.Value) && !string.Equals(extraction.Variation.Value, item.Variation, StringComparison.OrdinalIgnoreCase)) conflicts.Add("Variation not confirmed");
        return conflicts;
    }

    private static string NormalizeCardNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var chars = value.Trim().ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray();
        return new string(chars);
    }
}

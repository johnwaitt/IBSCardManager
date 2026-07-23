using System.Collections.Concurrent;
using IBSCardManager.Data;
using IBSCardManager.Entities;
using IBSCardManager.Models;
using IBSCardManager.Options;
using IBSCardManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IBSCardManager.Controllers;

public class ScannerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IOpenAiCardAnalysisService _openAiCardAnalysis;
    private readonly ICardMetadataExtractionService _metadataExtractionService;
    private readonly ICardCandidateMatchingService _candidateMatchingService;
    private readonly ICardWebSearchService _cardWebSearchService;
    private readonly OpenAiCardAnalysisOptions _openAiOptions;
    private readonly IAnalyticsRecalculationQueue _analyticsQueue;
    private readonly IKnowledgeService _knowledgeService;
    private readonly IKnowledgeEvidenceService _knowledgeEvidenceService;
    private readonly IKnowledgeCorrectionService _knowledgeCorrectionService;
    private readonly IKnowledgeLearningService _knowledgeLearningService;
    private readonly IDecisionHistoryService _decisionHistoryService;
    private readonly IConfidenceScoringService _confidenceScoringService;
    private readonly IKnowledgeModelVersionService _knowledgeModelVersionService;
    private readonly IApplicationVersionProvider _applicationVersionProvider;
    private readonly ICatalogVersionService _catalogVersionService;

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly ConcurrentDictionary<string, ScannerSelectedCatalogCardDto> SelectedCatalogByPair = new(StringComparer.OrdinalIgnoreCase);

    public ScannerController(
        ApplicationDbContext context,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IOpenAiCardAnalysisService openAiCardAnalysis,
        ICardMetadataExtractionService metadataExtractionService,
        ICardCandidateMatchingService candidateMatchingService,
        ICardWebSearchService cardWebSearchService,
        IOptions<OpenAiCardAnalysisOptions> openAiOptions,
        IAnalyticsRecalculationQueue analyticsQueue,
        IKnowledgeService knowledgeService,
        IKnowledgeEvidenceService knowledgeEvidenceService,
        IKnowledgeCorrectionService knowledgeCorrectionService,
        IKnowledgeLearningService knowledgeLearningService,
        IDecisionHistoryService decisionHistoryService,
        IConfidenceScoringService confidenceScoringService,
        IKnowledgeModelVersionService knowledgeModelVersionService,
        IApplicationVersionProvider applicationVersionProvider,
        ICatalogVersionService catalogVersionService)
    {
        _context = context;
        _configuration = configuration;
        _environment = environment;
        _openAiCardAnalysis = openAiCardAnalysis;
        _metadataExtractionService = metadataExtractionService;
        _candidateMatchingService = candidateMatchingService;
        _cardWebSearchService = cardWebSearchService;
        _openAiOptions = openAiOptions.Value;
        _analyticsQueue = analyticsQueue;
        _knowledgeService = knowledgeService;
        _knowledgeEvidenceService = knowledgeEvidenceService;
        _knowledgeCorrectionService = knowledgeCorrectionService;
        _knowledgeLearningService = knowledgeLearningService;
        _decisionHistoryService = decisionHistoryService;
        _confidenceScoringService = confidenceScoringService;
        _knowledgeModelVersionService = knowledgeModelVersionService;
        _applicationVersionProvider = applicationVersionProvider;
        _catalogVersionService = catalogVersionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await BuildScannerModelAsync());
    }

    [HttpGet]
    public IActionResult Preview(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return NotFound();
        var intakeFolder = GetIntakeFolder();
        if (!Directory.Exists(intakeFolder)) return NotFound();
        var fullPath = Path.Combine(intakeFolder, Path.GetFileName(fileName));
        if (!System.IO.File.Exists(fullPath)) return NotFound();
        return PhysicalFile(fullPath, GetContentType(fullPath));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadScans(List<IFormFile> files)
    {
        var folder = GetIntakeFolder();
        if (string.IsNullOrWhiteSpace(folder))
        {
            TempData["ScannerMessage"] = "Configure ScannerImport:IntakeFolder in appsettings.json first.";
            return RedirectToAction(nameof(Index));
        }

        Directory.CreateDirectory(folder);
        var saved = 0;
        foreach (var file in files.Where(x => x.Length > 0))
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)) continue;
            var stem = Path.GetFileNameWithoutExtension(file.FileName);
            var safeStem = string.Concat(stem.Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-'));
            var destination = Path.Combine(folder, $"{safeStem}-{DateTime.Now:yyyyMMddHHmmssfff}{extension}");
            await using var stream = System.IO.File.Create(destination);
            await file.CopyToAsync(stream);
            saved++;
        }

        TempData["ScannerMessage"] = $"{saved} scan image(s) added to the inbox.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> SearchMatches(string? query, string? player, int? year, string? set, string? cardNumber, string? team)
    {
        query = query?.Trim();
        player = player?.Trim();
        set = set?.Trim();
        cardNumber = cardNumber?.Trim();
        team = team?.Trim();

        var tokens = string.IsNullOrWhiteSpace(query)
            ? Array.Empty<string>()
            : query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => x.Length > 1)
                .Take(8)
                .ToArray();

        var cards = _context.Cards.AsNoTracking().AsQueryable();
        foreach (var searchToken in tokens)
        {
            var token = searchToken;
            cards = cards.Where(x =>
                x.Subject.Contains(token) ||
                (x.Team != null && x.Team.Contains(token)) ||
                (x.Set != null && x.Set.Contains(token)) ||
                (x.CardNumber != null && x.CardNumber.Contains(token)) ||
                (x.Variety != null && x.Variety.Contains(token)) ||
                (x.Year.HasValue && x.Year.Value.ToString().Contains(token)));
        }
        if (!string.IsNullOrWhiteSpace(player)) cards = cards.Where(x => x.Subject.Contains(player));
        if (year.HasValue) cards = cards.Where(x => x.Year == year);
        if (!string.IsNullOrWhiteSpace(set)) cards = cards.Where(x => x.Set != null && x.Set.Contains(set));
        if (!string.IsNullOrWhiteSpace(cardNumber)) cards = cards.Where(x => x.CardNumber != null && x.CardNumber.Contains(cardNumber));
        if (!string.IsNullOrWhiteSpace(team)) cards = cards.Where(x => x.Team != null && x.Team.Contains(team));

        var inventory = await cards.OrderBy(x => x.Subject).Take(12)
            .Select(x => new
            {
                type = "inventory",
                id = x.CardId,
                checklistItemId = x.ChecklistItemId,
                player = x.Subject,
                team = x.Team,
                year = x.Year,
                set = x.Set,
                cardNumber = x.CardNumber,
                variety = x.Variety,
                quantity = x.Quantity,
                image = x.ImageSourcePreference == "Stock" && x.StockImageUrl != null ? x.StockImageUrl : x.FrontImagePath
            }).ToListAsync();

        var checklistQuery = _context.ChecklistItems.AsNoTracking().Include(x => x.Product).AsQueryable();
        foreach (var searchToken in tokens)
        {
            var token = searchToken;
            checklistQuery = checklistQuery.Where(x =>
                x.Subject.Contains(token) ||
                (x.Team != null && x.Team.Contains(token)) ||
                x.CardNumber.Contains(token) ||
                (x.Subset != null && x.Subset.Contains(token)) ||
                (x.Product != null && (x.Product.DisplayName.Contains(token) || x.Product.Year.ToString().Contains(token))));
        }
        if (!string.IsNullOrWhiteSpace(player)) checklistQuery = checklistQuery.Where(x => x.Subject.Contains(player));
        if (year.HasValue) checklistQuery = checklistQuery.Where(x => x.Product != null && x.Product.Year == year);
        if (!string.IsNullOrWhiteSpace(set)) checklistQuery = checklistQuery.Where(x => x.Product != null && x.Product.DisplayName.Contains(set));
        if (!string.IsNullOrWhiteSpace(cardNumber)) checklistQuery = checklistQuery.Where(x => x.CardNumber.Contains(cardNumber));
        if (!string.IsNullOrWhiteSpace(team)) checklistQuery = checklistQuery.Where(x => x.Team != null && x.Team.Contains(team));

        var checklist = await checklistQuery.OrderBy(x => x.Subject).Take(12)
            .Select(x => new
            {
                type = "checklist",
                id = x.ChecklistItemId,
                productId = x.ProductId,
                player = x.Subject,
                team = x.Team,
                year = x.Product != null ? x.Product.Year : (int?)null,
                set = x.Product != null ? x.Product.DisplayName : null,
                cardNumber = x.CardNumber,
                variety = x.Subset,
                quantity = 0,
                image = x.StockImageUrl
            }).ToListAsync();

        return Json(inventory.Cast<object>().Concat(checklist.Cast<object>()));
    }


    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(25_000_000)]
    public async Task<IActionResult> SaveAcceptedImage(IFormFile file, string side, string sourceFileName)
    {
        if (file == null || file.Length == 0) return BadRequest("No accepted image was received.");
        if (side is not ("front" or "back")) return BadRequest("The image side is invalid.");

        var intakeFolder = GetIntakeFolder();
        if (string.IsNullOrWhiteSpace(intakeFolder)) return BadRequest("The scanner intake folder is not configured.");
        Directory.CreateDirectory(intakeFolder);

        var sourceStem = Path.GetFileNameWithoutExtension(
    Path.GetFileName(sourceFileName ?? string.Empty));

        sourceStem = System.Text.RegularExpressions.Regex.Replace(
            sourceStem,
            @"-(front|back)-accepted-\d+",
            string.Empty,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        var safeStem = string.Concat(
            sourceStem.Select(ch =>
                char.IsLetterOrDigit(ch) || ch is '-' or '_'
                    ? ch
                    : '-'));

        safeStem = safeStem.Trim('-', '_');

        if (string.IsNullOrWhiteSpace(safeStem))
        {
            safeStem = Guid.NewGuid().ToString("N");
        }

        // Guard against path-too-long: suffix is ~40 chars; NTFS filename limit is 255.
        const int maxStemLength = 200;
        if (safeStem.Length > maxStemLength)
            safeStem = safeStem[..maxStemLength].TrimEnd('-', '_');

        var fileName =
            $"{safeStem}-{side}-accepted-{DateTime.UtcNow:yyyyMMddHHmmssfff}.jpg";
        var path = Path.Combine(intakeFolder, fileName);
        await using (var stream = System.IO.File.Create(path))
        {
            await file.CopyToAsync(stream);
        }

        return Json(new
        {
            fileName,
            previewUrl = Url.Action(nameof(Preview), "Scanner", new { fileName })
        });
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AnalyzePair(
        string frontFileName,
        string? backFileName,
        Guid? productId,
        string? player,
        string? team,
        int? year,
        string? manufacturer,
        string? brand,
        string? product,
        string? cardNumber,
        string? checklistSection,
        string? parallel,
        string? variation,
        string? serialNumber,
        int? serialMaximum,
        bool? isRookie,
        bool? isAutograph,
        bool? isRelic,
        bool? isPatch,
        bool consentApproved,
        bool? alwaysAllow,
        CancellationToken cancellationToken)
    {
        if (!_openAiCardAnalysis.IsConfigured)
            return BadRequest(new { configured = false, message = "ChatGPT image analysis is not configured." });

        if (!_openAiOptions.EnableChatGptAnalysis || _openAiOptions.LocalAnalysisOnly)
            return BadRequest(new { configured = _openAiCardAnalysis.IsConfigured, message = "ChatGPT image analysis is disabled in scanner settings." });

        if (_openAiOptions.AskBeforeUpload && !consentApproved)
            return BadRequest(new { configured = _openAiCardAnalysis.IsConfigured, code = "consent-required", message = "ChatGPT analysis will securely send the selected card images to OpenAI for identification." });

        if (alwaysAllow == true)
        {
            _openAiOptions.AskBeforeUpload = false;
        }

        if (string.IsNullOrWhiteSpace(frontFileName)) return BadRequest("Accept a front image first.");
        var intakeFolder = GetIntakeFolder();
        var frontPath = Path.Combine(intakeFolder, Path.GetFileName(frontFileName));
        var backPath = string.IsNullOrWhiteSpace(backFileName) ? null : Path.Combine(intakeFolder, Path.GetFileName(backFileName));
        if (!System.IO.File.Exists(frontPath)) return NotFound("The accepted front image was not found.");

        var request = new ScannerIdentificationRequest
        {
            FrontFileName = frontFileName,
            BackFileName = backFileName,
            FrontPath = frontPath,
            BackPath = backPath,
            Hints = new ScannerExtractionHints
            {
                Player = player,
                Team = team,
                Year = year,
                Manufacturer = manufacturer,
                Brand = brand,
                Product = product,
                CardNumber = cardNumber,
                ChecklistSection = checklistSection,
                Parallel = parallel,
                Variation = variation,
                SerialNumber = serialNumber,
                SerialMaximum = serialMaximum,
                IsRookie = isRookie,
                IsAutograph = isAutograph,
                IsRelic = isRelic,
                IsPatch = isPatch
            }
        };

        try
        {
            var extraction = await _metadataExtractionService.ExtractAsync(request, cancellationToken);
            var candidates = await _candidateMatchingService.FindCandidatesAsync(extraction, productId, cancellationToken);
            return Json(new
            {
                extraction,
                candidates,
                pairId = BuildPairId(frontFileName, backFileName),
                configured = _openAiCardAnalysis.IsConfigured,
                cached = extraction.UsedCachedAnalysis
            });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { configured = _openAiCardAnalysis.IsConfigured, message = exception.Message });
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmSelectedCandidate(string pairId, [FromBody] ScannerCandidateResult selectedCandidate)
    {
        if (string.IsNullOrWhiteSpace(pairId)) return BadRequest(new { message = "A scanner pair id is required." });
        if (selectedCandidate == null || selectedCandidate.ChecklistItemId == null)
            return BadRequest(new { message = "Select a candidate before confirming." });

        var selectedDto = new ScannerSelectedCatalogCardDto
        {
            ProductId = selectedCandidate.ProductId,
            ChecklistCardId = selectedCandidate.ChecklistItemId,
            Player = selectedCandidate.Player,
            Team = selectedCandidate.Team,
            Year = selectedCandidate.Year,
            Manufacturer = selectedCandidate.Manufacturer,
            Brand = selectedCandidate.Brand,
            Product = selectedCandidate.Product,
            ChecklistSection = selectedCandidate.ChecklistSection,
            CardNumber = selectedCandidate.CardNumber,
            Parallel = selectedCandidate.Parallel,
            Variation = selectedCandidate.Variation,
            IsRookie = selectedCandidate.IsRookie,
            IsAutograph = selectedCandidate.IsAutograph,
            IsRelic = selectedCandidate.IsRelic,
            IsPatch = selectedCandidate.IsPatch,
            SerialMaximum = selectedCandidate.SerialMaximum,
            CandidateSource = selectedCandidate.CatalogSource
        };

        SelectedCatalogByPair[pairId] = selectedDto;

        var versionInfo = _knowledgeModelVersionService.GetCurrentVersions();
        var candidateConfidence = decimal.Clamp(selectedCandidate.Confidence * 100m, 0m, 100m);
        var confidenceInput = new ConfidenceScoringInput
        {
            StableIdMatch = selectedCandidate.ChecklistItemId.HasValue,
            CardNumberMatch = !string.IsNullOrWhiteSpace(selectedCandidate.CardNumber),
            PlayerMatch = !string.IsNullOrWhiteSpace(selectedCandidate.Player),
            TeamMatch = !string.IsNullOrWhiteSpace(selectedCandidate.Team),
            ProductMatch = !string.IsNullOrWhiteSpace(selectedCandidate.Product),
            YearMatch = selectedCandidate.Year.HasValue,
            ParallelMatch = !string.IsNullOrWhiteSpace(selectedCandidate.Parallel),
            VariationMatch = !string.IsNullOrWhiteSpace(selectedCandidate.Variation),
            OcrQuality = candidateConfidence,
            ImageQuality = candidateConfidence,
            UserConfirmations = 1,
            UserCorrections = 0,
            MarketplaceSupportCount = 0,
            ContradictingEvidenceCount = selectedCandidate.Conflicts?.Count ?? 0,
            MissingRequiredFieldCount = string.IsNullOrWhiteSpace(selectedCandidate.CardNumber) || string.IsNullOrWhiteSpace(selectedCandidate.Player) ? 1 : 0,
            SourceFreshnessDays = 0
        };

        var score = _confidenceScoringService.Calculate(confidenceInput);
        var knowledgeRecordId = BuildKnowledgeStableId(KnowledgeType.ChecklistMatch, pairId);

        var knowledgeRecord = await _knowledgeService.UpsertKnowledgeRecordAsync(new KnowledgeRecord
        {
            StableId = knowledgeRecordId,
            KnowledgeType = KnowledgeType.ChecklistMatch,
            SubjectType = KnowledgeSubjectType.ScannerPair,
            SubjectStableId = pairId,
            StatementKey = "selected-checklist-item",
            StatementValue = selectedCandidate.ChecklistItemId.Value.ToString(),
            NormalizedValue = selectedCandidate.ChecklistItemId.Value.ToString("N"),
            ConfidenceScore = score.Score,
            VerificationLevel = ResolveVerificationLevel(score, selectedCandidate),
            SourceCount = 1,
            UserConfirmationCount = 1,
            UserCorrectionCount = 0,
            MarketplaceConfirmationCount = 0,
            CatalogConfirmationCount = 1,
            ImageMatchConfirmationCount = 1,
            FirstObservedAt = DateTime.UtcNow,
            LastObservedAt = DateTime.UtcNow,
            LastVerifiedAt = score.Classification is ConfidenceClassification.Verified or ConfidenceClassification.VeryHigh ? DateTime.UtcNow : null,
            ModelVersion = versionInfo.AiModelName,
            RuleVersion = versionInfo.ConfidenceRuleVersion
        });

        await _knowledgeEvidenceService.AddEvidenceAsync(new KnowledgeEvidence
        {
            KnowledgeRecordId = knowledgeRecord.Id,
            EvidenceType = KnowledgeEvidenceType.UserConfirmation,
            SourceType = KnowledgeEvidenceSourceType.User,
            SourceRecordId = selectedCandidate.ChecklistItemId.Value.ToString(),
            EvidenceSummary = "Scanner candidate selected by user confirmation action.",
            RawValue = selectedCandidate.Player,
            NormalizedValue = selectedCandidate.Player?.Trim().ToLowerInvariant(),
            ConfidenceContribution = 4m,
            IsSupporting = true,
            IsContradicting = false,
            ObservedAt = DateTime.UtcNow,
            ModelVersion = versionInfo.AiModelName,
            RuleVersion = versionInfo.ConfidenceRuleVersion
        });

        var explanation = _decisionHistoryService.BuildExplanationSummary(new DecisionExplanationInput
        {
            SelectedOption = selectedCandidate.ChecklistItemId.Value.ToString(),
            ConfidenceScore = score.Score,
            StrongestSupportingFactors = score.SupportingFactors.Select(x => x.Name).Take(3).ToArray(),
            ImportantContradictions = score.ContradictingFactors.Select(x => x.Name).Take(2).ToArray(),
            MissingInformation = score.MissingDataFactors.Select(x => x.Name).Take(2).ToArray(),
            AlternativesConsidered = currentCandidateAlternatives(selectedCandidate),
            UserAction = "Confirmed candidate"
        });

        await _decisionHistoryService.RecordDecisionAsync(new DecisionHistoryRecord
        {
            SubjectType = KnowledgeSubjectType.ScannerPair,
            SubjectStableId = pairId,
            DecisionType = DecisionType.CandidateSelection,
            DecisionStatus = DecisionStatus.Confirmed,
            SelectedOption = selectedCandidate.ChecklistItemId.Value.ToString(),
            AlternativeOptionsJson = System.Text.Json.JsonSerializer.Serialize(currentCandidateAlternatives(selectedCandidate)),
            ConfidenceScore = score.Score,
            ExplanationSummary = explanation,
            EvidenceCount = 1,
            MissingDataSummary = score.MissingDataFactors.Any() ? string.Join(", ", score.MissingDataFactors.Select(x => x.Name).Take(3)) : null,
            UserAction = "Candidate confirmed in scanner workflow",
            CompletedAt = DateTime.UtcNow,
            ModelVersion = versionInfo.AiModelName,
            PromptVersion = versionInfo.PromptTemplateVersion,
            RuleVersion = score.RuleVersion,
            ApplicationVersion = _applicationVersionProvider.ApplicationVersion,
            CatalogVersion = await _catalogVersionService.GetCatalogVersionAsync()
        });

        var comparison = BuildComparisonRows(selectedCandidate);
        return Json(new
        {
            selected = selectedDto,
            comparison,
            state = "Candidate selected",
            confidence = score.Score,
            confidenceClassification = score.Classification.ToString()
        });

        static IReadOnlyList<string> currentCandidateAlternatives(ScannerCandidateResult candidate)
            => candidate.MatchReasons.Any() ? candidate.MatchReasons.ToArray() : Array.Empty<string>();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckDuplicates([FromBody] ScannerDuplicateCheckRequest request)
    {
        if (request == null) return BadRequest(new { message = "Duplicate check input is required." });

        var warnings = new List<ScannerDuplicateWarning>();

        if (request.ChecklistCardId.HasValue)
        {
            var exactChecklist = await _context.Cards.AsNoTracking()
                .Where(x => x.ChecklistItemId == request.ChecklistCardId.Value)
                .Select(x => new { x.CardId, x.ChecklistItemId })
                .FirstOrDefaultAsync();

            if (exactChecklist != null)
            {
                warnings.Add(new ScannerDuplicateWarning
                {
                    Category = "Same catalog card already owned",
                    Message = "This checklist card already exists in inventory.",
                    ExistingCardId = exactChecklist.CardId,
                    ChecklistItemId = exactChecklist.ChecklistItemId,
                    IsExactMatch = true
                });
            }
        }

        var player = request.Player?.Trim();
        var cardNumber = request.CardNumber?.Trim();
        if (request.ProductId.HasValue && !string.IsNullOrWhiteSpace(player) && !string.IsNullOrWhiteSpace(cardNumber))
        {
            var possibleCopy = await _context.Cards.AsNoTracking()
                .Where(x => x.ProductId == request.ProductId && x.Subject == player && x.CardNumber == cardNumber)
                .Select(x => new { x.CardId, x.ChecklistItemId })
                .FirstOrDefaultAsync();

            if (possibleCopy != null)
            {
                warnings.Add(new ScannerDuplicateWarning
                {
                    Category = "Possible existing copy",
                    Message = "A similar inventory card already exists for this product/player/card number.",
                    ExistingCardId = possibleCopy.CardId,
                    ChecklistItemId = possibleCopy.ChecklistItemId,
                    IsExactMatch = false
                });
            }
        }

        var frontFileName = Path.GetFileName(request.FrontFileName ?? request.FrontPath ?? string.Empty);
        var backFileName = Path.GetFileName(request.BackFileName ?? request.BackPath ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(frontFileName))
        {
            var sameImage = await _context.Cards.AsNoTracking()
                .Where(x => x.FrontImagePath != null && x.FrontImagePath.Contains(frontFileName))
                .Select(x => x.CardId)
                .FirstOrDefaultAsync();

            if (sameImage != Guid.Empty)
            {
                warnings.Add(new ScannerDuplicateWarning
                {
                    Category = "Same image already imported",
                    Message = "The front image appears to have been imported before.",
                    ExistingCardId = sameImage,
                    IsExactMatch = true
                });
            }
        }

        if (!string.IsNullOrWhiteSpace(request.PairId) && SelectedCatalogByPair.ContainsKey(request.PairId))
        {
            warnings.Add(new ScannerDuplicateWarning
            {
                Category = "Exact scanner pair already processed",
                Message = "This scanner pair has already been confirmed in this session.",
                IsExactMatch = true
            });
        }

        return Json(new ScannerDuplicateCheckResult { Warnings = warnings });
    }

    [HttpGet]
    public async Task<IActionResult> SearchWebForCard(
        string? player,
        int? year,
        string? manufacturer,
        string? product,
        string? cardNumber,
        string? team,
        string? parallel,
        string? variation,
        Guid? productId,
        CancellationToken cancellationToken)
    {
        var request = new WebCardSearchRequest
        {
            ProductId = productId,
            Player = player,
            Year = year,
            Manufacturer = manufacturer,
            Product = product,
            CardNumber = cardNumber,
            Team = team,
            Parallel = parallel,
            Variation = variation
        };

        var query = string.Join(" ", new[]
        {
            player,
            year?.ToString(),
            manufacturer,
            product,
            cardNumber,
            team,
            parallel,
            variation,
            "front back"
        }.Where(value => !string.IsNullOrWhiteSpace(value)));

        var candidates = await _cardWebSearchService.SearchCardAsync(request, cancellationToken);

        foreach (var candidate in candidates)
        {
            _context.WebSearchResults.Add(new WebSearchResult
            {
                ProductId = productId,
                SearchScope = "Card",
                SearchQuery = string.IsNullOrWhiteSpace(candidate.SearchQuery) ? query : candidate.SearchQuery,
                Title = candidate.Title,
                PageSource = candidate.PageSource,
                PageUrl = candidate.PageUrl,
                ImageUrl = candidate.ImageUrl,
                DateRetrievedUtc = DateTime.UtcNow,
                UserConfirmed = false,
                MetadataJson = "{}"
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Json(new
        {
            query,
            candidates,
            requiresUserConfirmation = true,
            autoCreateChecklistDisabled = true
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> IdentifyWithChatGpt(string frontFileName, string? backFileName, bool consentApproved, CancellationToken cancellationToken)
    {
        if (!_openAiCardAnalysis.IsConfigured)
            return BadRequest(new { configured = false, message = "ChatGPT image analysis is not configured." });

        if (!_openAiOptions.EnableChatGptAnalysis || _openAiOptions.LocalAnalysisOnly)
            return BadRequest(new { configured = _openAiCardAnalysis.IsConfigured, message = "ChatGPT image analysis is disabled in scanner settings." });

        if (_openAiOptions.AskBeforeUpload && !consentApproved)
            return BadRequest(new { configured = _openAiCardAnalysis.IsConfigured, code = "consent-required", message = "ChatGPT analysis will securely send the selected card images to OpenAI for identification." });

        if (string.IsNullOrWhiteSpace(frontFileName)) return BadRequest("Accept a front image first.");
        var intakeFolder = GetIntakeFolder();
        var frontPath = Path.Combine(intakeFolder, Path.GetFileName(frontFileName));
        var backPath = string.IsNullOrWhiteSpace(backFileName) ? null : Path.Combine(intakeFolder, Path.GetFileName(backFileName));
        if (!System.IO.File.Exists(frontPath)) return NotFound("The accepted front image was not found.");

        try
        {
            var analysisResponse = await _openAiCardAnalysis.AnalyzeAsync(new CardAnalysisRequest
            {
                FrontImagePath = frontPath,
                BackImagePath = backPath,
                PairId = BuildPairId(frontFileName, backFileName)
            }, cancellationToken);

            return Json(new
            {
                configured = true,
                cached = analysisResponse.Cached,
                model = analysisResponse.Model,
                result = analysisResponse.Analysis
            });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { configured = _openAiCardAnalysis.IsConfigured, message = exception.Message });
        }
    }

    [HttpGet]
    public IActionResult GetScannerAiSettings()
    {
        return Json(new ScannerAiSettingsInput
        {
            EnableChatGptAnalysis = _openAiOptions.EnableChatGptAnalysis,
            AskBeforeUpload = _openAiOptions.AskBeforeUpload,
            LocalAnalysisOnly = _openAiOptions.LocalAnalysisOnly,
            AllowTextOnlyOnlineSearch = _openAiOptions.AllowTextOnlyOnlineSearch,
            ReuseCachedAnalysis = _openAiOptions.ReuseCachedAnalysis,
            VisionModel = _openAiOptions.VisionModel,
            TimeoutSeconds = _openAiOptions.TimeoutSeconds,
            MaxRetries = _openAiOptions.MaxRetries,
            MaxImageBytes = _openAiOptions.MaxImageBytes
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SaveScannerAiSettings([FromBody] ScannerAiSettingsInput input)
    {
        if (input == null) return BadRequest(new { message = "Scanner AI settings payload is required." });

        _openAiOptions.EnableChatGptAnalysis = input.EnableChatGptAnalysis;
        _openAiOptions.AskBeforeUpload = input.AskBeforeUpload;
        _openAiOptions.LocalAnalysisOnly = input.LocalAnalysisOnly;
        _openAiOptions.AllowTextOnlyOnlineSearch = input.AllowTextOnlyOnlineSearch;
        _openAiOptions.ReuseCachedAnalysis = input.ReuseCachedAnalysis;
        _openAiOptions.VisionModel = string.IsNullOrWhiteSpace(input.VisionModel) ? _openAiOptions.VisionModel : input.VisionModel.Trim();
        _openAiOptions.TimeoutSeconds = Math.Max(15, input.TimeoutSeconds);
        _openAiOptions.MaxRetries = Math.Max(0, input.MaxRetries);
        _openAiOptions.MaxImageBytes = Math.Max(1_000_000, input.MaxImageBytes);

        return Json(new { saved = true });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TestScannerAiConnection(CancellationToken cancellationToken)
    {
        var ok = await _openAiCardAnalysis.TestConnectionAsync(cancellationToken);
        return ok
            ? Json(new { ok = true, message = "Connection succeeded." })
            : BadRequest(new { ok = false, message = "ChatGPT image analysis is not configured." });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(ScannerImportViewModel model)
    {
        var useStockImage = string.Equals(model.PreferredImageSource, "Stock", StringComparison.OrdinalIgnoreCase);
        if (!useStockImage && string.IsNullOrWhiteSpace(model.FrontFileName))
            ModelState.AddModelError(nameof(model.FrontFileName), "Select a front scan or choose the stock image option.");
        if (useStockImage && string.IsNullOrWhiteSpace(model.StockImageUrl))
            ModelState.AddModelError(nameof(model.StockImageUrl), "Enter or select a stock image URL.");
        if (string.IsNullOrWhiteSpace(model.Subject))
            ModelState.AddModelError(nameof(model.Subject), "Enter the player name.");
        if ((model.Destination is "Checklist" or "Both") && !model.ProductId.HasValue)
            ModelState.AddModelError(nameof(model.ProductId), "Choose the catalog set before adding to a checklist.");

        if (model.MatchedChecklistItemId.HasValue && !model.ConfirmedChecklistItemId.HasValue)
            ModelState.AddModelError(nameof(model.ConfirmedChecklistItemId), "Confirm the selected candidate card before saving inventory.");

        if (model.HasUserCorrections && string.IsNullOrWhiteSpace(model.CorrectionFieldName))
            ModelState.AddModelError(nameof(model.CorrectionFieldName), "Correction field name is required when corrections are submitted.");

        if (!ModelState.IsValid)
        {
            var refreshed = await BuildScannerModelAsync();
            CopyFormValues(model, refreshed);
            return View("Index", refreshed);
        }

        try
        {
            ChecklistItem? checklistItem = null;
            var confirmedChecklistId = model.ConfirmedChecklistItemId ?? model.MatchedChecklistItemId;
            if (confirmedChecklistId.HasValue)
                checklistItem = await _context.ChecklistItems.Include(x => x.Product).FirstOrDefaultAsync(x => x.ChecklistItemId == confirmedChecklistId.Value);

            if (model.Destination is "Checklist" or "Both")
            {
                checklistItem ??= await _context.ChecklistItems.FirstOrDefaultAsync(x =>
                    x.ProductId == model.ProductId!.Value &&
                    x.CardNumber == (model.CardNumber ?? string.Empty) &&
                    x.Subject == model.Subject!.Trim());

                if (checklistItem == null)
                {
                    checklistItem = new ChecklistItem
                    {
                        ProductId = model.ProductId!.Value,
                        CardNumber = model.CardNumber?.Trim() ?? string.Empty,
                        Subject = model.Subject!.Trim(),
                        Team = model.Team?.Trim(),
                        Subset = model.Variety?.Trim(),
                        IsRookie = model.IsRookie,
                        IsAutograph = model.IsAutograph,
                        IsRelic = model.IsRelic,
                        StockImageUrl = model.StockImageUrl?.Trim()
                    };
                    _context.ChecklistItems.Add(checklistItem);
                }
                else
                {
                    checklistItem.Team = model.Team?.Trim();
                    checklistItem.Subset = model.Variety?.Trim();
                    checklistItem.IsRookie = model.IsRookie;
                    checklistItem.IsAutograph = model.IsAutograph;
                    checklistItem.IsRelic = model.IsRelic;
                    if (!string.IsNullOrWhiteSpace(model.StockImageUrl)) checklistItem.StockImageUrl = model.StockImageUrl.Trim();
                }
            }

            if (model.Destination is "Inventory" or "Both")
            {
                Card? card = null;
                if (model.MatchedInventoryCardId.HasValue)
                    card = await _context.Cards.FirstOrDefaultAsync(x => x.CardId == model.MatchedInventoryCardId.Value);

                if (card != null)
                {
                    card.Quantity += Math.Max(1, model.Quantity);
                    card.ModifiedDate = DateTime.Now;
                }
                else
                {
                    var cardId = Guid.NewGuid();
                    var frontPath = string.IsNullOrWhiteSpace(model.FrontFileName) ? null : CopyScannerImage(model.FrontFileName, cardId, "front");
                    var backPath = string.IsNullOrWhiteSpace(model.BackFileName) ? null : CopyScannerImage(model.BackFileName, cardId, "back");
                    var product = model.ProductId.HasValue ? await _context.Products.FindAsync(model.ProductId.Value) : null;
                    card = new Card
                    {
                        CardId = cardId,
                        ProductId = product?.ProductId ?? checklistItem?.ProductId,
                        ChecklistItem = checklistItem,
                        Subject = model.Subject!.Trim(),
                        Team = model.Team?.Trim(),
                        Year = model.Year ?? product?.Year,
                        Category = model.Category?.Trim() ?? "Baseball",
                        Set = model.Set?.Trim() ?? product?.DisplayName,
                        CardNumber = model.CardNumber?.Trim(),
                        Variety = model.Variety?.Trim(),
                        Serial = model.Serial?.Trim(),
                        IsRookie = model.IsRookie,
                        IsAutograph = model.IsAutograph,
                        IsRelic = model.IsRelic,
                        Quantity = Math.Max(1, model.Quantity),
                        FrontImagePath = frontPath,
                        BackImagePath = backPath,
                        StockImageUrl = model.StockImageUrl?.Trim() ?? checklistItem?.StockImageUrl,
                        ImageSourcePreference = useStockImage ? "Stock" : "Scan",
                        ListingStatus = "Not Listed",
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };
                    _context.Cards.Add(card);
                }
            }

            await _context.SaveChangesAsync();
            await _analyticsQueue.EnqueueAsync("inventory-change");

            if (!string.IsNullOrWhiteSpace(model.ScannerPairId))
            {
                SelectedCatalogByPair[model.ScannerPairId] = SelectedCatalogByPair.TryGetValue(model.ScannerPairId, out var selected)
                    ? selected
                    : new ScannerSelectedCatalogCardDto
                    {
                        ProductId = model.ProductId,
                        ChecklistCardId = model.ConfirmedChecklistItemId ?? model.MatchedChecklistItemId,
                        Player = model.Subject,
                        Team = model.Team,
                        Year = model.Year,
                        Product = model.Set,
                        CardNumber = model.CardNumber,
                        Parallel = model.Variety,
                        CandidateSource = model.CandidateSource ?? "local-catalog"
                    };
            }

            if (model.HasUserCorrections)
            {
                var versionInfo = _knowledgeModelVersionService.GetCurrentVersions();
                var subjectStableId = string.IsNullOrWhiteSpace(model.ScannerPairId)
                    ? BuildPairId(model.FrontFileName ?? string.Empty, model.BackFileName)
                    : model.ScannerPairId!;

                var correction = await _knowledgeCorrectionService.CreateCorrectionAsync(new UserCorrection
                {
                    SubjectType = KnowledgeSubjectType.ScannerPair,
                    SubjectStableId = subjectStableId,
                    OriginalValue = model.CorrectionOriginalValue,
                    CorrectedValue = model.CorrectionCorrectedValue ?? string.Empty,
                    FieldName = model.CorrectionFieldName ?? "Unknown",
                    CorrectionType = ParseCorrectionType(model.CorrectionType),
                    Reason = model.CorrectionReason,
                    UserNotes = model.CorrectionNotes,
                    AppliedToCurrentRecord = true,
                    EligibleForLearning = true,
                    LearningStatus = LearningStatus.PendingReview,
                    ModelVersion = versionInfo.AiModelName,
                    RuleVersion = versionInfo.LearningRuleVersion
                });

                var learningDecision = _knowledgeLearningService.EvaluateAutoLearningAction(correction.CorrectionType, repeatedCorrectionCount: 1, highImpactAction: false);
                if (learningDecision.QueueForReview && learningDecision.ReviewItemType.HasValue)
                {
                    _context.KnowledgeReviewItems.Add(new KnowledgeReviewItem
                    {
                        ItemType = learningDecision.ReviewItemType.Value,
                        Status = KnowledgeReviewQueueState.New,
                        SubjectType = KnowledgeSubjectType.ScannerPair,
                        SubjectStableId = correction.SubjectStableId,
                        Summary = $"Correction queued: {correction.FieldName}",
                        Notes = learningDecision.Reason,
                        RuleVersion = versionInfo.LearningRuleVersion
                    });
                    await _context.SaveChangesAsync();
                }
            }

            TempData["InventoryMessage"] = model.Destination switch
            {
                "Checklist" => "Card added to the reference checklist.",
                "Both" => "Card added to the checklist and inventory.",
                _ => "Card added to inventory."
            };

            return model.Destination == "Checklist"
                ? RedirectToAction("Index", "Checklists", new { productId = model.ProductId })
                : RedirectToAction("Index", "Cards");
        }
        catch (Exception exception)
        {
            ModelState.AddModelError(string.Empty, "The scan could not be imported. " + exception.Message);
            var refreshed = await BuildScannerModelAsync();
            CopyFormValues(model, refreshed);
            return View("Index", refreshed);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteTemporaryFile(string fileName)
    {
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            var path = Path.Combine(GetIntakeFolder(), Path.GetFileName(fileName));
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<ScannerImportViewModel> BuildScannerModelAsync()
    {
        var model = new ScannerImportViewModel
        {
            EnableChatGptAnalysis = _openAiOptions.EnableChatGptAnalysis,
            AskBeforeChatGptUpload = _openAiOptions.AskBeforeUpload,
            LocalAnalysisOnly = _openAiOptions.LocalAnalysisOnly,
            AllowTextOnlyOnlineSearch = _openAiOptions.AllowTextOnlyOnlineSearch,
            ReuseCachedAnalysis = _openAiOptions.ReuseCachedAnalysis,
            VisionModel = _openAiOptions.VisionModel,
            TimeoutSeconds = _openAiOptions.TimeoutSeconds,
            MaxRetries = _openAiOptions.MaxRetries,
            MaxImageBytes = _openAiOptions.MaxImageBytes
        };
        model.Products = await _context.Products.AsNoTracking().OrderByDescending(x => x.Year).ThenBy(x => x.DisplayName)
            .Select(x => new SelectListItem(x.DisplayName, x.ProductId.ToString())).ToListAsync();

        if (!_openAiCardAnalysis.IsConfigured)
        {
            model.Message = "ChatGPT image analysis is not configured. Set OpenAI:ApiKey with user secrets or environment variables.";
        }

        var folder = GetIntakeFolder();
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
        {
            model.Message = "Scanner intake folder was not found: " + folder;
            return model;
        }

        var files = Directory.EnumerateFiles(folder)
            .Where(file => AllowedExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
            .Select(file => new FileInfo(file)).OrderByDescending(file => file.LastWriteTime).ToList();

        model.Images = files.Select(file => new ScannerImageItem
        {
            FileName = file.Name,
            PreviewUrl = Url?.Action(nameof(Preview), "Scanner", new { fileName = file.Name }) ?? $"/Scanner/Preview?fileName={Uri.EscapeDataString(file.Name)}",
            ModifiedDate = file.LastWriteTime,
            SizeBytes = file.Length
        }).ToList();

        if (files.Count > 0) model.FrontFileName = files[0].Name;
        if (files.Count > 1) model.BackFileName = files[1].Name;
        return model;
    }

    private string CopyScannerImage(string sourceFileName, Guid cardId, string side)
    {
        var source = Path.Combine(GetIntakeFolder(), Path.GetFileName(sourceFileName));
        if (!System.IO.File.Exists(source)) throw new FileNotFoundException("Scanner image was not found.", source);
        var extension = Path.GetExtension(source).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)) throw new InvalidOperationException("Unsupported image type.");
        var relativeFolder = _configuration["ScannerImport:PermanentFolder"] ?? Path.Combine("uploads", "cards");
        var physicalFolder = Path.Combine(_environment.WebRootPath, relativeFolder);
        Directory.CreateDirectory(physicalFolder);
        var fileName = $"{cardId:N}-{side}{extension}";
        System.IO.File.Copy(source, Path.Combine(physicalFolder, fileName), true);
        return $"/{relativeFolder.Replace("\\", "/").Trim('/')}/{fileName}";
    }

    private static string BuildPairId(string frontFileName, string? backFileName)
    {
        var front = Path.GetFileName(frontFileName);
        var back = Path.GetFileName(backFileName ?? string.Empty);
        return string.IsNullOrWhiteSpace(back) ? front : $"{front}|{back}";
    }

    private static string BuildKnowledgeStableId(KnowledgeType type, string subjectStableId)
    {
        return $"knowledge:{type}:{subjectStableId}".ToLowerInvariant();
    }

    private static KnowledgeVerificationLevel ResolveVerificationLevel(ConfidenceScoreResult score, ScannerCandidateResult selectedCandidate)
    {
        if ((selectedCandidate.Conflicts?.Count ?? 0) > 0)
        {
            return KnowledgeVerificationLevel.Disputed;
        }

        if (score.Classification == ConfidenceClassification.Verified)
        {
            return KnowledgeVerificationLevel.Verified;
        }

        if (score.SupportingFactors.Count >= 2)
        {
            return KnowledgeVerificationLevel.MultiSourceSupported;
        }

        return KnowledgeVerificationLevel.UserConfirmed;
    }

    private static CorrectionType ParseCorrectionType(string? correctionType)
    {
        if (string.IsNullOrWhiteSpace(correctionType))
        {
            return CorrectionType.Other;
        }

        return correctionType.Trim().ToLowerInvariant() switch
        {
            "identity" => CorrectionType.Identity,
            "product" => CorrectionType.Product,
            "set" => CorrectionType.Set,
            "cardnumber" => CorrectionType.CardNumber,
            "player" => CorrectionType.Player,
            "team" => CorrectionType.Team,
            "parallel" => CorrectionType.Parallel,
            "variation" => CorrectionType.Variation,
            "year" => CorrectionType.Year,
            "manufacturer" => CorrectionType.Manufacturer,
            "ocr" => CorrectionType.Ocr,
            "imageorientation" => CorrectionType.ImageOrientation,
            "pricing" => CorrectionType.Pricing,
            _ => CorrectionType.Other
        };
    }

    private static IReadOnlyList<ScannerComparisonRow> BuildComparisonRows(ScannerCandidateResult candidate)
    {
        return new[]
        {
            BuildComparisonRow("Player", candidate.Player, candidate.Player),
            BuildComparisonRow("Team", candidate.Team, candidate.Team),
            BuildComparisonRow("Year", candidate.Year?.ToString(), candidate.Year?.ToString()),
            BuildComparisonRow("Product", candidate.Product, candidate.Product),
            BuildComparisonRow("Card Number", candidate.CardNumber, candidate.CardNumber),
            BuildComparisonRow("Parallel", candidate.Parallel, candidate.Parallel),
            BuildComparisonRow("Variation", candidate.Variation, candidate.Variation),
            BuildComparisonRow("Rookie", candidate.IsRookie.ToString(), candidate.IsRookie.ToString()),
            BuildComparisonRow("Autograph", candidate.IsAutograph.ToString(), candidate.IsAutograph.ToString()),
            BuildComparisonRow("Relic", candidate.IsRelic.ToString(), candidate.IsRelic.ToString()),
            BuildComparisonRow("Serial Maximum", candidate.SerialMaximum?.ToString(), candidate.SerialMaximum?.ToString())
        };
    }

    private static ScannerComparisonRow BuildComparisonRow(string field, string? extractedValue, string? catalogValue)
    {
        var normalizedExtracted = extractedValue?.Trim();
        var normalizedCatalog = catalogValue?.Trim();

        var result = string.IsNullOrWhiteSpace(normalizedExtracted) || string.IsNullOrWhiteSpace(normalizedCatalog)
            ? "Unknown"
            : string.Equals(normalizedExtracted, normalizedCatalog, StringComparison.OrdinalIgnoreCase)
                ? "Match"
                : "Conflict";

        return new ScannerComparisonRow
        {
            Field = field,
            ExtractedValue = extractedValue,
            CatalogValue = catalogValue,
            Result = result
        };
    }

    private string GetIntakeFolder() => _configuration["ScannerImport:IntakeFolder"] ?? string.Empty;
    private static string GetContentType(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "application/octet-stream"
    };

    private static void CopyFormValues(ScannerImportViewModel source, ScannerImportViewModel destination)
    {
        destination.FrontFileName = source.FrontFileName; destination.BackFileName = source.BackFileName;
        destination.Subject = source.Subject; destination.Team = source.Team; destination.Year = source.Year;
        destination.Category = source.Category; destination.Set = source.Set; destination.CardNumber = source.CardNumber;
        destination.Variety = source.Variety; destination.Serial = source.Serial; destination.IsRookie = source.IsRookie;
        destination.IsAutograph = source.IsAutograph; destination.IsRelic = source.IsRelic; destination.Quantity = source.Quantity;
        destination.StockImageUrl = source.StockImageUrl; destination.PreferredImageSource = source.PreferredImageSource;
        destination.Destination = source.Destination; destination.ProductId = source.ProductId;
        destination.MatchedChecklistItemId = source.MatchedChecklistItemId; destination.MatchedInventoryCardId = source.MatchedInventoryCardId;
        destination.ConfirmedChecklistItemId = source.ConfirmedChecklistItemId; destination.ScannerPairId = source.ScannerPairId;
        destination.ScannerPairState = source.ScannerPairState; destination.FrontImageHash = source.FrontImageHash;
        destination.BackImageHash = source.BackImageHash; destination.CombinedImageHash = source.CombinedImageHash;
        destination.IdentificationConfidence = source.IdentificationConfidence; destination.CandidateSource = source.CandidateSource;
    }
}

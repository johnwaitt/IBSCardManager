using IBSCardManager.Controllers;
using IBSCardManager.Data;
using IBSCardManager.Entities;
using IBSCardManager.Services;
using IBSCardManager.Models;
using IBSCardManager.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Xunit;

namespace IBSCardManager.Tests;

public sealed class ScannerControllerTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private ServiceProvider _provider = null!;
    private ApplicationDbContext _context = null!;
    private ScannerController _controller = null!;
    private ICardCandidateMatchingService _candidateMatcher = null!;
    private FakeOpenAiCardAnalysisService _fakeOpenAi = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
        services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment());
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ScannerImport:IntakeFolder"] = AppContext.BaseDirectory,
            ["ScannerImport:PermanentFolder"] = "uploads/cards",
            ["OpenAI:ApiKey"] = "test-key",
            ["OpenAI:VisionModel"] = "gpt-4.1-mini",
            ["OpenAI:TimeoutSeconds"] = "30",
            ["OpenAI:MaxRetries"] = "1"
        }).Build());
        services.Configure<OpenAiCardAnalysisOptions>(options =>
        {
            options.ApiKey = "test-key";
            options.VisionModel = "gpt-4.1-mini";
            options.TimeoutSeconds = 30;
            options.MaxRetries = 1;
            options.MaxImageBytes = 8 * 1024 * 1024;
            options.ReuseCachedAnalysis = true;
            options.EnableChatGptAnalysis = true;
            options.AskBeforeUpload = false;
        });
        services.AddMemoryCache();
        _fakeOpenAi = new FakeOpenAiCardAnalysisService();
        services.AddSingleton<IOpenAiCardAnalysisService>(_fakeOpenAi);
        services.AddScoped<ICardMetadataExtractionService, CardImageIdentificationService>();
        services.AddScoped<ICardCandidateMatchingService, CardImageIdentificationService>();
        services.AddScoped<ICardImageIdentificationService, CardImageIdentificationService>();
        services.AddScoped<ICardWebSearchService, TestCardWebSearchService>();
        services.AddSingleton<IAnalyticsRecalculationQueue, TestAnalyticsRecalculationQueue>();
        services.AddSingleton<IKnowledgeService, TestKnowledgeService>();
        services.AddSingleton<IKnowledgeQueryService, TestKnowledgeQueryService>();
        services.AddSingleton<IKnowledgeLearningService, KnowledgeLearningService>();
        services.AddSingleton<IKnowledgeEvidenceService, TestKnowledgeEvidenceService>();
        services.AddSingleton<IKnowledgeCorrectionService, TestKnowledgeCorrectionService>();
        services.AddSingleton<IConfidenceScoringService, ConfidenceScoringService>();
        services.AddSingleton<IDecisionHistoryService, TestDecisionHistoryService>();
        services.AddSingleton<IKnowledgeModelVersionService, KnowledgeModelVersionService>();
        services.AddSingleton<IApplicationVersionProvider>(new StaticVersionProvider("2.2.0", "2.2.0-tests"));
        services.AddSingleton<ICatalogVersionService, TestCatalogVersionService>();

        _provider = services.BuildServiceProvider();
        _context = _provider.GetRequiredService<ApplicationDbContext>();
        await _context.Database.EnsureCreatedAsync();
        SeedData(_context);
        await _context.SaveChangesAsync();
        _candidateMatcher = _provider.GetRequiredService<ICardCandidateMatchingService>();

        _controller = new ScannerController(
            _context,
            _provider.GetRequiredService<IConfiguration>(),
            _provider.GetRequiredService<IWebHostEnvironment>(),
            _provider.GetRequiredService<IOpenAiCardAnalysisService>(),
            _provider.GetRequiredService<ICardMetadataExtractionService>(),
            _candidateMatcher,
            _provider.GetRequiredService<ICardWebSearchService>(),
            _provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenAiCardAnalysisOptions>>(),
            _provider.GetRequiredService<IAnalyticsRecalculationQueue>(),
            _provider.GetRequiredService<IKnowledgeService>(),
            _provider.GetRequiredService<IKnowledgeEvidenceService>(),
            _provider.GetRequiredService<IKnowledgeCorrectionService>(),
            _provider.GetRequiredService<IKnowledgeLearningService>(),
            _provider.GetRequiredService<IDecisionHistoryService>(),
            _provider.GetRequiredService<IConfidenceScoringService>(),
            _provider.GetRequiredService<IKnowledgeModelVersionService>(),
            _provider.GetRequiredService<IApplicationVersionProvider>(),
            _provider.GetRequiredService<ICatalogVersionService>())
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), new EmptyTempDataProvider())
        };
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _provider.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task SearchMatches_RanksExactPlayerAndCardNumberAboveBroadMatches()
    {
        var result = await _controller.SearchMatches("Darrell Hernaiz 125", "Darrell Hernaiz", 2024, "Heritage", "125", "Athletics");
        var json = Assert.IsType<JsonResult>(result);
        var items = Assert.IsAssignableFrom<IEnumerable<object>>(json.Value);
        Assert.NotEmpty(items);
    }

    [Fact]
    public async Task SearchMatches_ReturnsChecklistCandidates_ForPlayerAndCardNumber()
    {
        var result = await _controller.SearchMatches(null, "Darrell Hernaiz", 2024, "Heritage", "125", "Athletics");
        var json = Assert.IsType<JsonResult>(result);
        var items = Assert.IsAssignableFrom<IEnumerable<object>>(json.Value);
        Assert.NotEmpty(items);
    }

    [Fact]
    public async Task CandidateRanking_ExactPlayerAndCardNumber_RanksFirst()
    {
        var extraction = new ScannerIdentificationResult
        {
            Player = new ScannerExtractionField { Value = "Darrell Hernaiz" },
            CardNumber = new ScannerExtractionField { Value = "125" },
            Product = new ScannerExtractionField { Value = "2024 Topps Heritage" },
            Team = new ScannerExtractionField { Value = "Athletics" },
            Year = new ScannerExtractionField { Value = "2024" }
        };

        var candidates = await _candidateMatcher.FindCandidatesAsync(extraction, null, CancellationToken.None);

        Assert.NotEmpty(candidates);
        Assert.Equal("Darrell Hernaiz", candidates[0].Player);
        Assert.Equal("125", candidates[0].CardNumber);
    }

    [Fact]
    public async Task CandidateRanking_ProductOnlyResult_StaysLowConfidence()
    {
        var extraction = new ScannerIdentificationResult
        {
            Product = new ScannerExtractionField { Value = "2024 Topps Heritage" }
        };

        var candidates = await _candidateMatcher.FindCandidatesAsync(extraction, null, CancellationToken.None);

        Assert.NotEmpty(candidates);
        Assert.All(candidates, candidate => Assert.True(candidate.Confidence <= 64m));
    }

    [Fact]
    public async Task CandidateRanking_PlayerConflict_AppliesPenalty()
    {
        var extraction = new ScannerIdentificationResult
        {
            Player = new ScannerExtractionField { Value = "Wrong Player" },
            CardNumber = new ScannerExtractionField { Value = "125" }
        };

        var candidates = await _candidateMatcher.FindCandidatesAsync(extraction, null, CancellationToken.None);

        Assert.NotEmpty(candidates);
        Assert.Contains(candidates[0].Conflicts, conflict => conflict.Contains("Player differs", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ConfirmSelectedCandidate_ReturnsCandidateSelectedState()
    {
        var candidate = new ScannerCandidateResult
        {
            ChecklistItemId = _context.ChecklistItems.Select(x => x.ChecklistItemId).First(),
            ProductId = _context.Products.Select(x => x.ProductId).First(),
            Player = "Darrell Hernaiz",
            CardNumber = "125",
            Product = "2024 Topps Heritage"
        };

        var result = await _controller.ConfirmSelectedCandidate("pair-1", candidate);

        var json = Assert.IsType<JsonResult>(result);
        Assert.NotNull(json.Value);
    }

    [Fact]
    public async Task CheckDuplicates_ByChecklistCardId_ReturnsWarning()
    {
        var checklistId = _context.ChecklistItems.Select(x => x.ChecklistItemId).First();

        var result = await _controller.CheckDuplicates(new ScannerDuplicateCheckRequest
        {
            ChecklistCardId = checklistId,
            Player = "Darrell Hernaiz",
            CardNumber = "125"
        });

        var json = Assert.IsType<JsonResult>(result);
        var payload = Assert.IsType<ScannerDuplicateCheckResult>(json.Value);
        Assert.NotEmpty(payload.Warnings);
    }

    [Fact]
    public async Task Import_DoesNotProceed_WhenMatchedCandidateNotConfirmed()
    {
        var checklistId = _context.ChecklistItems.Select(x => x.ChecklistItemId).First();
        var beforeCount = await _context.Cards.CountAsync();

        var result = await _controller.Import(new ScannerImportViewModel
        {
            FrontFileName = "front.jpg",
            Subject = "Darrell Hernaiz",
            Destination = "Inventory",
            Quantity = 1,
            MatchedChecklistItemId = checklistId,
            ProductId = _context.Products.Select(x => x.ProductId).First()
        });

        Assert.IsType<ViewResult>(result);
        Assert.Equal(beforeCount, await _context.Cards.CountAsync());
    }

    [Fact]
    public async Task AnalyzePair_RequiresConsent_WhenAskBeforeUploadEnabled()
    {
        _controller.SaveScannerAiSettings(new ScannerAiSettingsInput
        {
            EnableChatGptAnalysis = true,
            AskBeforeUpload = true,
            LocalAnalysisOnly = false,
            AllowTextOnlyOnlineSearch = true,
            ReuseCachedAnalysis = true,
            VisionModel = "gpt-4.1-mini",
            TimeoutSeconds = 30,
            MaxRetries = 1,
            MaxImageBytes = 8 * 1024 * 1024
        });

        var front = CreateTempImage("consent-front.jpg");

        var result = await _controller.AnalyzePair(
            frontFileName: front,
            backFileName: null,
            productId: null,
            player: null,
            team: null,
            year: null,
            manufacturer: null,
            brand: null,
            product: null,
            cardNumber: null,
            checklistSection: null,
            parallel: null,
            variation: null,
            serialNumber: null,
            serialMaximum: null,
            isRookie: null,
            isAutograph: null,
            isRelic: null,
            isPatch: null,
            consentApproved: false,
            alwaysAllow: null,
            cancellationToken: CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AnalyzePair_ReturnsNotConfigured_WhenApiKeyMissing()
    {
        _fakeOpenAi.IsConfigured = false;
        var front = CreateTempImage("missing-key-front.jpg");

        var result = await _controller.AnalyzePair(
            frontFileName: front,
            backFileName: null,
            productId: null,
            player: null,
            team: null,
            year: null,
            manufacturer: null,
            brand: null,
            product: null,
            cardNumber: null,
            checklistSection: null,
            parallel: null,
            variation: null,
            serialNumber: null,
            serialMaximum: null,
            isRookie: null,
            isAutograph: null,
            isRelic: null,
            isPatch: null,
            consentApproved: true,
            alwaysAllow: null,
            cancellationToken: CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        _fakeOpenAi.IsConfigured = true;
    }

    [Fact]
    public async Task IdentifyWithChatGpt_UsesFakeAnalysisResponse()
    {
        _fakeOpenAi.NextResponse = new CardAnalysisResponseEnvelope
        {
            Cached = true,
            Model = "gpt-4.1-mini",
            Analysis = new CardAnalysisResult
            {
                PlayerName = new CardFieldResult<string> { Value = "Darrell Hernaiz", Confidence = 0.99m, EvidenceSource = CardEvidenceSource.Both },
                Team = new CardFieldResult<string> { Value = "Athletics", Confidence = 0.99m, EvidenceSource = CardEvidenceSource.Both },
                Product = new CardFieldResult<string> { Value = "2024 Topps Heritage", Confidence = 0.96m, EvidenceSource = CardEvidenceSource.Back },
                CardNumber = new CardFieldResult<string> { Value = "125", Confidence = 0.94m, EvidenceSource = CardEvidenceSource.Back },
                Confidence = 0.95m
            }
        };

        var front = CreateTempImage("identify-front.jpg");
        var result = await _controller.IdentifyWithChatGpt(front, null, true, CancellationToken.None);

        var json = Assert.IsType<JsonResult>(result);
        Assert.NotNull(json.Value);
        Assert.Equal(1, _fakeOpenAi.Calls);
    }

    [Fact]
    public async Task SearchWebForCard_ReturnsQueryAndCandidates_WithoutAutoImport()
    {
        var result = await _controller.SearchWebForCard(
            player: "Darrell Hernaiz",
            year: 2024,
            manufacturer: "Topps",
            product: "Heritage",
            cardNumber: "125",
            team: "Athletics",
            parallel: null,
            variation: null,
            productId: _context.Products.Select(x => x.ProductId).First(),
            cancellationToken: CancellationToken.None);

        var json = Assert.IsType<JsonResult>(result);
        var payloadJson = System.Text.Json.JsonSerializer.Serialize(json.Value);
        Assert.Contains("query", payloadJson, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("requiresUserConfirmation", payloadJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ConfirmSelectedCandidate_Returns_DecisionConfidence_Metadata()
    {
        var result = await _controller.ConfirmSelectedCandidate("pair-1", new ScannerCandidateResult
        {
            ChecklistItemId = _context.ChecklistItems.Select(x => x.ChecklistItemId).First(),
            ProductId = _context.Products.Select(x => x.ProductId).First(),
            Player = "Darrell Hernaiz",
            Team = "Athletics",
            Year = 2024,
            Product = "2024 Topps Heritage",
            CardNumber = "125",
            Confidence = 0.86m,
            MatchReasons = new[] { "Exact card number", "Player match" }
        });

        var json = Assert.IsType<JsonResult>(result);
        var payloadJson = System.Text.Json.JsonSerializer.Serialize(json.Value);
        Assert.Contains("confidence", payloadJson, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("confidenceClassification", payloadJson, StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateTempImage(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, fileName);
        File.WriteAllBytes(path, new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 });
        return fileName;
    }

    private static void SeedData(ApplicationDbContext context)
    {
        var baseball = context.Sports.Single();
        var topps = context.Brands.Single(brand => brand.BrandName == "Topps");
        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            Year = 2024,
            ProductName = "Heritage",
            DisplayName = "2024 Topps Heritage",
            SportId = baseball.SportId,
            BrandId = topps.BrandId,
            IsActive = true
        };

        context.Products.Add(product);
        context.ChecklistItems.Add(new ChecklistItem
        {
            ChecklistItemId = Guid.NewGuid(),
            ProductId = product.ProductId,
            CardNumber = "125",
            Subject = "Darrell Hernaiz",
            Team = "Athletics",
            Subset = "Base",
            StockImageUrl = "https://example.com/125.jpg"
        });
        context.Cards.Add(new Card
        {
            CardId = Guid.NewGuid(),
            ProductId = product.ProductId,
            ChecklistItemId = context.ChecklistItems.Local.First().ChecklistItemId,
            Subject = "Darrell Hernaiz",
            Team = "Athletics",
            Year = 2024,
            Set = "2024 Topps Heritage",
            CardNumber = "125",
            Quantity = 1,
            Category = "Baseball"
        });
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "IBSCardManager.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public string EnvironmentName { get; set; } = "Development";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = AppContext.BaseDirectory;
    }

    private sealed class FakeOpenAiCardAnalysisService : IOpenAiCardAnalysisService
    {
        public bool IsConfigured { get; set; } = true;
        public int Calls { get; private set; }
        public CardAnalysisRequest? LastRequest { get; private set; }
        public CardAnalysisResponseEnvelope? NextResponse { get; set; }

        public Task<CardAnalysisResponseEnvelope> AnalyzeAsync(CardAnalysisRequest request, CancellationToken cancellationToken = default)
        {
            Calls++;
            LastRequest = request;
            if (!IsConfigured) throw new InvalidOperationException("ChatGPT image analysis is not configured.");

            var response = NextResponse ?? new CardAnalysisResponseEnvelope
            {
                Cached = false,
                Model = "gpt-4.1-mini",
                Analysis = new CardAnalysisResult
                {
                    PlayerName = new CardFieldResult<string> { Value = "Darrell Hernaiz", Confidence = 0.95m, EvidenceSource = CardEvidenceSource.Both },
                    Team = new CardFieldResult<string> { Value = "Athletics", Confidence = 0.9m, EvidenceSource = CardEvidenceSource.Both },
                    Product = new CardFieldResult<string> { Value = "2024 Topps Heritage", Confidence = 0.85m, EvidenceSource = CardEvidenceSource.Back },
                    CardNumber = new CardFieldResult<string> { Value = "125", Confidence = 0.85m, EvidenceSource = CardEvidenceSource.Back },
                    Confidence = 0.9m
                }
            };

            return Task.FromResult(response);
        }

        public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default) => Task.FromResult(IsConfigured);
    }

    private sealed class EmptyTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    private sealed class StaticVersionProvider : IApplicationVersionProvider
    {
        public StaticVersionProvider(string applicationVersion, string informationalVersion)
        {
            ApplicationVersion = applicationVersion;
            InformationalVersion = informationalVersion;
        }

        public string ApplicationVersion { get; }
        public string InformationalVersion { get; }
    }

    private sealed class TestCatalogVersionService : ICatalogVersionService
    {
        public Task<string?> GetCatalogVersionAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>("test-catalog-v1");
        public Task<DateTime?> GetCatalogUpdatedAtAsync(CancellationToken cancellationToken = default) => Task.FromResult<DateTime?>(DateTime.UtcNow);
    }

    private sealed class TestKnowledgeService : IKnowledgeService
    {
        public Task<KnowledgeRecord> UpsertKnowledgeRecordAsync(KnowledgeRecord record, CancellationToken cancellationToken = default)
        {
            record.Id = record.Id == Guid.Empty ? Guid.NewGuid() : record.Id;
            return Task.FromResult(record);
        }
    }

    private sealed class TestKnowledgeEvidenceService : IKnowledgeEvidenceService
    {
        public Task<KnowledgeEvidence> AddEvidenceAsync(KnowledgeEvidence evidence, CancellationToken cancellationToken = default)
        {
            evidence.Id = evidence.Id == Guid.Empty ? Guid.NewGuid() : evidence.Id;
            return Task.FromResult(evidence);
        }

        public Task<IReadOnlyList<KnowledgeEvidence>> GetEvidenceForRecordAsync(Guid knowledgeRecordId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<KnowledgeEvidence>>(Array.Empty<KnowledgeEvidence>());
    }

    private sealed class TestKnowledgeCorrectionService : IKnowledgeCorrectionService
    {
        public Task<UserCorrection> CreateCorrectionAsync(UserCorrection correction, CancellationToken cancellationToken = default)
        {
            correction.Id = correction.Id == Guid.Empty ? Guid.NewGuid() : correction.Id;
            return Task.FromResult(correction);
        }

        public Task<UserCorrection> UpdateLearningStatusAsync(Guid correctionId, LearningStatus status, string? note, CancellationToken cancellationToken = default)
            => Task.FromResult(new UserCorrection { Id = correctionId, LearningStatus = status, CorrectedValue = "test", FieldName = "field", SubjectStableId = "subject" });
    }

    private sealed class TestDecisionHistoryService : IDecisionHistoryService
    {
        public Task<DecisionHistoryRecord> RecordDecisionAsync(DecisionHistoryRecord record, CancellationToken cancellationToken = default)
        {
            record.Id = record.Id == Guid.Empty ? Guid.NewGuid() : record.Id;
            return Task.FromResult(record);
        }

        public string BuildExplanationSummary(DecisionExplanationInput input)
            => $"Selected: {input.SelectedOption}; Confidence: {input.ConfidenceScore}.";
    }

    private sealed class TestKnowledgeQueryService : IKnowledgeQueryService
    {
        public Task<KnowledgeRecord?> GetByStableIdAsync(string stableId, CancellationToken cancellationToken = default)
            => Task.FromResult<KnowledgeRecord?>(null);

        public Task<IReadOnlyList<KnowledgeRecord>> GetBySubjectAsync(KnowledgeSubjectType subjectType, string subjectStableId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<KnowledgeRecord>>(Array.Empty<KnowledgeRecord>());
    }
}

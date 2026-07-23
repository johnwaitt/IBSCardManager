using IBSCardManager.Data;
using IBSCardManager.Entities;
using IBSCardManager.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IBSCardManager.Tests;

public sealed class MasterCatalogCompatibilityTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private ServiceProvider _provider = null!;
    private ApplicationDbContext _context = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:CardManagerConnection"] = "Data Source=IBSCardManager.db;Database=IBSCardManager"
        };

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(settings).Build());
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
        services.AddScoped<IMasterCatalogService, LocalMasterCatalogService>();
        services.AddScoped<ICatalogLookupService, LocalCatalogLookupService>();
        services.AddScoped<ICatalogSearchService, LocalCatalogSearchService>();
        services.AddScoped<ICatalogVersionService, LocalCatalogVersionService>();
        services.AddScoped<ICatalogImportService, LocalCatalogImportService>();
        services.AddScoped<ICatalogImageService, LocalCatalogImageService>();
        services.AddScoped<ICatalogDatabaseProvider, LocalCatalogDatabaseProvider>();
        services.AddScoped<ICatalogValidationService, CatalogValidationService>();
        services.AddSingleton<IApplicationVersionProvider>(new StaticVersionProvider("2.2.0", "2.2.0-tests"));
        services.AddScoped<IKnowledgeModelVersionService, KnowledgeModelVersionService>();
        services.AddScoped<IKnowledgeHealthService, KnowledgeHealthService>();
        services.Configure<IBSCardManager.Options.OpenAiCardAnalysisOptions>(_ => { });
        services.AddScoped<IDiagnosticsService, DiagnosticsService>();
        services.AddScoped<IBackupManifestService, BackupManifestService>();

        _provider = services.BuildServiceProvider();
        _context = _provider.GetRequiredService<ApplicationDbContext>();
        await _context.Database.EnsureCreatedAsync();

        Seed(_context);
        await _context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _provider.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task Catalog_Service_Boundaries_Are_Resolvable()
    {
        using var scope = _provider.CreateScope();
        Assert.NotNull(scope.ServiceProvider.GetService<IMasterCatalogService>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICatalogLookupService>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICatalogSearchService>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICatalogVersionService>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICatalogImportService>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICatalogValidationService>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICatalogImageService>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICatalogDatabaseProvider>());
    }

    [Fact]
    public async Task Stable_Identifiers_Are_Preserved_On_Inventory()
    {
        var card = await _context.Cards.AsNoTracking().FirstAsync();
        Assert.NotNull(card.ProductId);
        Assert.NotNull(card.ChecklistItemId);
    }

    [Fact]
    public async Task Catalog_Validation_Returns_Readiness_Issues_Or_Empty()
    {
        using var scope = _provider.CreateScope();
        var validation = scope.ServiceProvider.GetRequiredService<ICatalogValidationService>();

        var issues = await validation.RunReadinessChecksAsync();
        Assert.NotNull(issues);
    }

    [Fact]
    public async Task Diagnostics_Reports_Version_And_Provider()
    {
        using var scope = _provider.CreateScope();
        var diagnostics = scope.ServiceProvider.GetRequiredService<IDiagnosticsService>();

        var model = await diagnostics.BuildDiagnosticsAsync();
        Assert.Contains("2.2.0", model.ApplicationVersion);
        Assert.Equal("Ver 2.2.0", model.UpperLeftDisplayVersion);
        Assert.Equal("knowledge-schema-v1", model.KnowledgeSchemaVersion);
        Assert.Equal("knowledge-confidence-rules-v1", model.ConfidenceRuleVersion);
        Assert.Equal("knowledge-learning-rules-v1", model.LearningRuleVersion);
        Assert.Equal("LocalSqlServerCatalogTables", model.CatalogProvider);
    }

    [Fact]
    public async Task Backup_Manifest_Contains_Runtime_Database_Record()
    {
        using var scope = _provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBackupManifestService>();

        var manifest = await service.GenerateManifestAsync();
        Assert.True(manifest.Databases.Count >= 2);
        Assert.Contains(manifest.Databases, x => x.DatabaseRole == "RuntimePrimary");
        Assert.Contains(manifest.Databases, x => x.DatabaseRole == "KnowledgeEngine");
        Assert.All(manifest.Databases, x => Assert.False(string.IsNullOrWhiteSpace(x.DatabaseName)));
        Assert.All(manifest.Databases.Where(x => x.VerificationResult), x => Assert.NotNull(x.ChecksumSha256));
    }

    [Fact]
    public void Layout_Displays_UpperLeft_Version_Label_220()
    {
        var root = AppContext.BaseDirectory;
        var cursor = new DirectoryInfo(root);
        while (cursor is not null && !File.Exists(Path.Combine(cursor.FullName, "Views", "Shared", "_Layout.cshtml")))
        {
            cursor = cursor.Parent;
        }

        Assert.NotNull(cursor);
        var layoutPath = Path.Combine(cursor!.FullName, "Views", "Shared", "_Layout.cshtml");
        var layout = File.ReadAllText(layoutPath);
        Assert.Contains("Ver @appVersion", layout);
        Assert.Contains("Administration", layout);
    }

    private static void Seed(ApplicationDbContext context)
    {
        var sport = context.Sports.Single();
        var brand = context.Brands.Single(x => x.BrandName == "Topps");

        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            Year = 2024,
            ProductName = "Heritage",
            DisplayName = "2024 Topps Heritage",
            SportId = sport.SportId,
            BrandId = brand.BrandId,
            IsActive = true,
            CatalogRecordId = "prod-2024-topps-heritage",
            CatalogSource = "LocalCatalog",
            CatalogSourceRecordId = "L-1000",
            CatalogVersion = "local-v1",
            CatalogUpdatedAt = DateTime.UtcNow,
            CatalogConfidence = 92m,
            IsVerified = true
        };

        var checklist = new ChecklistItem
        {
            ChecklistItemId = Guid.NewGuid(),
            ProductId = product.ProductId,
            CardNumber = "125",
            Subject = "Darrell Hernaiz",
            Team = "Athletics",
            CatalogRecordId = "chk-2024-topps-heritage-125",
            CatalogSource = "LocalCatalog",
            CatalogSourceRecordId = "C-125",
            CatalogVersion = "local-v1",
            CatalogUpdatedAt = DateTime.UtcNow,
            CatalogConfidence = 90m,
            IsVerified = true
        };

        var card = new Card
        {
            CardId = Guid.NewGuid(),
            ProductId = product.ProductId,
            ChecklistItemId = checklist.ChecklistItemId,
            Subject = "Darrell Hernaiz",
            Team = "Athletics",
            Category = "Baseball",
            CardNumber = "125",
            Set = "2024 Topps Heritage",
            Quantity = 1,
            MyCost = 8m,
            MyValue = 20m,
            CatalogRecordId = "inv-1",
            CatalogSource = "Checklist",
            CatalogSourceRecordId = "chk-2024-topps-heritage-125",
            CatalogVersion = "local-v1",
            CatalogUpdatedAt = DateTime.UtcNow,
            CatalogConfidence = 88m,
            IsVerified = true
        };

        context.Products.Add(product);
        context.ChecklistItems.Add(checklist);
        context.Cards.Add(card);
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
}


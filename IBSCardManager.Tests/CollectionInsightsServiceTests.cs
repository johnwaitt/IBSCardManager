using IBSCardManager.Data;
using IBSCardManager.Entities;
using IBSCardManager.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IBSCardManager.Tests;

public sealed class CollectionInsightsServiceTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private ServiceProvider _provider = null!;
    private ApplicationDbContext _context = null!;
    private ICollectionInsightsService _service = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
        services.AddSingleton<IApplicationVersionProvider>(new StaticVersionProvider("2.2.0", "2.2.0-tests"));
        services.AddScoped<ICollectionInsightsService, CollectionInsightsService>();

        _provider = services.BuildServiceProvider();
        _context = _provider.GetRequiredService<ApplicationDbContext>();
        await _context.Database.EnsureCreatedAsync();

        Seed(_context);
        await _context.SaveChangesAsync();

        _service = _provider.GetRequiredService<ICollectionInsightsService>();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _provider.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task Dashboard_Computes_Value_Cost_And_Unrealized()
    {
        await _service.RecalculateAnalyticsAsync("test");
        var model = await _service.BuildCollectionAnalyticsDashboardAsync("all");

        Assert.True(model.TotalCollectionValue > 0m);
        Assert.True(model.TotalCostBasis > 0m);
        Assert.Equal(model.TotalCollectionValue - model.TotalCostBasis, model.UnrealizedGainLoss);
    }

    [Fact]
    public async Task SnapshotCreation_Prevents_Duplicate_When_Unchanged()
    {
        var first = await _service.CreateSnapshotAsync("manual");
        var second = await _service.CreateSnapshotAsync("manual");

        Assert.True(first.Created);
        Assert.False(second.Created);
    }

    [Fact]
    public async Task Recommendations_Include_Explainability_Content()
    {
        await _service.RecalculateAnalyticsAsync("test");
        var center = await _service.BuildRecommendationCenterAsync();

        var any = center.GradeThese.Concat(center.SellThese).Concat(center.HoldThese).FirstOrDefault();
        Assert.NotNull(any);
        Assert.Contains("Inputs:", any!.Reason);
        Assert.NotEmpty(any.Risks);
    }

    [Fact]
    public async Task Duplicate_Analytics_Detects_Multiple_Copies()
    {
        var rows = await _service.GetDuplicateAnalyticsAsync();
        Assert.Contains(rows, row => row.Copies > 1);
    }

    [Fact]
    public async Task DataQuality_Score_Flags_Incomplete_Records()
    {
        var rows = await _service.GetDataQualityIssuesAsync();
        Assert.Contains(rows, row => row.Classification is "Needs review" or "Poor data quality");
    }

    [Fact]
    public async Task Concentration_Returns_Percentage_Breakdown()
    {
        var rows = await _service.GetConcentrationAsync("player");
        Assert.NotEmpty(rows);
        Assert.All(rows, row => Assert.True(row.PercentageOfCollectionValue >= 0m));
    }

    [Fact]
    public async Task TopReport_Includes_Roi_And_GradingOutputs()
    {
        await _service.RecalculateAnalyticsAsync("test");
        var rows = await _service.GetTopReportAsync("highestroi", 10);

        Assert.NotEmpty(rows);
        Assert.All(rows, row =>
        {
            Assert.True(row.Score >= -1000m);
            Assert.True(row.ExpectedGradedValue >= 0m);
        });
    }

    [Fact]
    public void Version_Is_Updated_To_220_In_DirectoryBuildProps()
    {
        var root = AppContext.BaseDirectory;
        var cursor = new DirectoryInfo(root);
        while (cursor is not null && !File.Exists(Path.Combine(cursor.FullName, "Directory.Build.props")))
        {
            cursor = cursor.Parent;
        }

        Assert.NotNull(cursor);
        var props = File.ReadAllText(Path.Combine(cursor!.FullName, "Directory.Build.props"));
        Assert.Contains("<AppVersion>2.2.0</AppVersion>", props);
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

    private static void Seed(ApplicationDbContext context)
    {
        var baseball = context.Sports.Single();
        var topps = context.Brands.Single(x => x.BrandName == "Topps");

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

        var checklist = new ChecklistItem
        {
            ChecklistItemId = Guid.NewGuid(),
            ProductId = product.ProductId,
            CardNumber = "125",
            Subject = "Darrell Hernaiz",
            Team = "Athletics"
        };

        context.ChecklistItems.Add(checklist);

        context.Cards.AddRange(
            new Card
            {
                CardId = Guid.NewGuid(),
                Subject = "Darrell Hernaiz",
                Team = "Athletics",
                Year = 2024,
                Set = "2024 Topps Heritage",
                ProductId = product.ProductId,
                ChecklistItemId = checklist.ChecklistItemId,
                CardNumber = "125",
                Quantity = 2,
                MyCost = 8m,
                MyValue = 24m,
                PsaEstimate = 60m,
                IsRookie = true,
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                ModifiedDate = DateTime.UtcNow.AddDays(-2)
            },
            new Card
            {
                CardId = Guid.NewGuid(),
                Subject = "Darrell Hernaiz",
                Team = "Athletics",
                Year = 2024,
                Set = "2024 Topps Heritage",
                ProductId = product.ProductId,
                ChecklistItemId = checklist.ChecklistItemId,
                CardNumber = "125",
                Quantity = 1,
                MyCost = 5m,
                MyValue = 10m,
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                ModifiedDate = DateTime.UtcNow.AddDays(-40)
            },
            new Card
            {
                CardId = Guid.NewGuid(),
                Subject = "Incomplete Card",
                Team = null,
                Year = 2023,
                Set = null,
                ProductId = null,
                ChecklistItemId = null,
                CardNumber = null,
                Quantity = 1,
                MyCost = null,
                MyValue = null,
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                ModifiedDate = DateTime.UtcNow.AddDays(-3)
            });
    }
}

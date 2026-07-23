using IBSCardManager.Data;
using IBSCardManager.Entities;
using IBSCardManager.Options;
using IBSCardManager.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace IBSCardManager.Tests;

public sealed class CollectionAnalyticsServiceTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private ServiceProvider _provider = null!;
    private ApplicationDbContext _context = null!;
    private ICollectionAnalyticsService _service = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.Configure<DashboardOptions>(options => options.CacheMinutes = 1);
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
        services.AddScoped<ICollectionAnalyticsService, CollectionAnalyticsService>();

        _provider = services.BuildServiceProvider();
        _context = _provider.GetRequiredService<ApplicationDbContext>();
        await _context.Database.EnsureCreatedAsync();
        SeedData(_context);
        await _context.SaveChangesAsync();
        _service = _provider.GetRequiredService<ICollectionAnalyticsService>();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _provider.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task DashboardAggregatesTotals()
    {
        var model = await _service.BuildDashboardAsync();

        Assert.Equal(7, model.TotalCards);
        Assert.Equal(4, model.TotalRecords);
        Assert.Contains(model.Metrics, metric => metric.Title == "Total Collection Value");
        Assert.Contains(model.Sections, section => section.Title == "Recently Added Cards");
    }

    [Fact]
    public async Task RecentlyAddedOrdersDescending()
    {
        var model = await _service.BuildExplorerAsync("recentlyadded", null, 1, 10, null);

        Assert.Equal("recentlyadded", model.Mode);
        Assert.True(model.Items.Count >= 2);
        Assert.Equal("Unmatched Card", model.Items[0].Title);
    }

    [Fact]
    public async Task PlayersExplorerGroupsByPlayerTotals()
    {
        var model = await _service.BuildExplorerAsync("players", null, 1, 10, null);

        var playerOne = Assert.Single(model.Items, item => item.Title == "Player One");
        Assert.Equal("5 pcs", playerOne.QuantityText);
    }

    [Fact]
    public async Task UnmatchedInventoryIsClassified()
    {
        var model = await _service.BuildExplorerAsync("unmatchedcards", null, 1, 10, null);

        Assert.Single(model.Items);
        Assert.Equal("Unmatched Card", model.Items[0].Title);
    }

    [Fact]
    public async Task CardsForSaleIsClassified()
    {
        var model = await _service.BuildExplorerAsync("cardsforsale", null, 1, 10, null);

        Assert.Single(model.Items);
        Assert.Equal("Player Two", model.Items[0].Title);
    }

    [Fact]
    public async Task SetsExplorerReturnsSetRows()
    {
        var model = await _service.BuildExplorerAsync("sets", null, 1, 10, null);

        Assert.Equal(2, model.TotalCount);
        Assert.Contains(model.Items, item => item.Title.Contains("Topps Chrome"));
    }

    private static void SeedData(ApplicationDbContext context)
    {
        var baseball = context.Sports.Single();
        var topps = context.Brands.Single(brand => brand.BrandName == "Topps");
        var bowman = context.Brands.Single(brand => brand.BrandName == "Bowman");

        var productOne = new Product
        {
            ProductId = Guid.NewGuid(),
            Year = 2024,
            ProductName = "Chrome",
            DisplayName = "2024 Topps Chrome",
            SportId = baseball.SportId,
            BrandId = topps.BrandId,
            IsActive = true
        };

        var productTwo = new Product
        {
            ProductId = Guid.NewGuid(),
            Year = 2023,
            ProductName = "Draft",
            DisplayName = "2023 Bowman Draft",
            SportId = baseball.SportId,
            BrandId = bowman.BrandId,
            IsActive = true
        };

        context.Products.AddRange(productOne, productTwo);

        var checklistOne = new ChecklistItem
        {
            ChecklistItemId = Guid.NewGuid(),
            ProductId = productOne.ProductId,
            CardNumber = "1",
            Subject = "Player One",
            Team = "Team A",
            Subset = "Base"
        };

        var checklistTwo = new ChecklistItem
        {
            ChecklistItemId = Guid.NewGuid(),
            ProductId = productTwo.ProductId,
            CardNumber = "2",
            Subject = "Player Two",
            Team = "Team B",
            Subset = "Base"
        };

        context.ChecklistItems.AddRange(checklistOne, checklistTwo);

        context.Cards.AddRange(
            new Card
            {
                CardId = Guid.NewGuid(),
                Subject = "Player One",
                Team = "Team A",
                Year = 2024,
                Set = "Topps Chrome",
                ProductId = productOne.ProductId,
                ChecklistItemId = checklistOne.ChecklistItemId,
                CardNumber = "1",
                Quantity = 4,
                MyValue = 25m,
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                ModifiedDate = DateTime.UtcNow.AddDays(-2)
            },
            new Card
            {
                CardId = Guid.NewGuid(),
                Subject = "Player One",
                Team = "Team A",
                Year = 2024,
                Set = "Topps Chrome",
                ProductId = productOne.ProductId,
                ChecklistItemId = checklistOne.ChecklistItemId,
                CardNumber = "1",
                Quantity = 1,
                MyValue = 30m,
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                ModifiedDate = DateTime.UtcNow.AddDays(-1)
            },
            new Card
            {
                CardId = Guid.NewGuid(),
                Subject = "Player Two",
                Team = "Team B",
                Year = 2023,
                Set = "Bowman Draft",
                ProductId = productTwo.ProductId,
                ChecklistItemId = checklistTwo.ChecklistItemId,
                CardNumber = "2",
                Quantity = 1,
                ListingStatus = "For Sale",
                ListingPrice = 12m,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            },
            new Card
            {
                CardId = Guid.NewGuid(),
                Subject = "Unmatched Card",
                Team = "Team C",
                Year = 2024,
                Set = "Unknown",
                Quantity = 1,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            }
        );
    }
}

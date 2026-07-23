using IBSCardManager.Controllers;
using IBSCardManager.Data;
using IBSCardManager.Entities;
using IBSCardManager.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace IBSCardManager.Tests;

public sealed class ChecklistsControllerTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private ServiceProvider _provider = null!;
    private ApplicationDbContext _context = null!;
    private ChecklistsController _controller = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
        services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment());

        _provider = services.BuildServiceProvider();
        _context = _provider.GetRequiredService<ApplicationDbContext>();
        await _context.Database.EnsureCreatedAsync();
        SeedData(_context);
        await _context.SaveChangesAsync();
        _controller = new ChecklistsController(
            _context,
            _provider.GetRequiredService<IWebHostEnvironment>(),
            new TestCardWebSearchService(),
            new TestChecklistCandidateService())
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
    public async Task Index_LoadsChecklistRows_ForQueryStringProductId()
    {
        var product = await _context.Products.SingleAsync(p => p.DisplayName == "2022 Topps Chrome Black");

        var result = await _controller.Index(product.ProductId, null);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ChecklistGridViewModel>(view.Model);

        Assert.Equal(product.ProductId, model.ProductId);
        Assert.Equal(product.DisplayName, model.ProductName);
        Assert.Equal(2, model.TotalCount);
        Assert.Equal(2, model.FilteredCount);
        Assert.Equal(2, model.Rows.Count);
        Assert.Equal("1", model.Rows[0].CardNumber);
        Assert.Equal("2", model.Rows[1].CardNumber);
    }

    [Fact]
    public async Task Index_ShowsEmptyState_ForProductWithoutChecklist()
    {
        var product = await _context.Products.SingleAsync(p => p.DisplayName == "2024 Topps Chrome Black");

        var result = await _controller.Index(product.ProductId, null);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ChecklistGridViewModel>(view.Model);

        Assert.Equal(product.ProductId, model.ProductId);
        Assert.Equal("This set exists, but its checklist has not been loaded.", model.EmptyMessage);
        Assert.Empty(model.Rows);
    }

    [Fact]
    public async Task Index_ReturnsPrompt_WhenNoProductIsSelected()
    {
        var result = await _controller.Index(null, null);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ChecklistGridViewModel>(view.Model);

        Assert.Equal("Select a set to view its checklist.", model.EmptyMessage);
        Assert.Null(model.ProductId);
        Assert.Empty(model.Rows);
    }

    [Fact]
    public async Task Index_ReturnsError_ForInvalidProduct()
    {
        var result = await _controller.Index(Guid.NewGuid(), null);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ChecklistGridViewModel>(view.Model);

        Assert.Equal("The selected set could not be found.", model.ErrorMessage);
        Assert.Empty(model.Rows);
    }

    [Fact]
    public async Task AddSetToCollection_LoadsChecklistRows_ForValidProduct()
    {
        var product = await _context.Products.SingleAsync(p => p.DisplayName == "2022 Topps Chrome Black");

        var result = await _controller.AddSetToCollection(product.ProductId);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<CollectionBuilderViewModel>(view.Model);

        Assert.Equal(product.ProductId, model.ProductId);
        Assert.Equal(product.DisplayName, model.ProductName);
        Assert.Equal(2, model.TotalChecklistCards);
        Assert.True(model.HasChecklistRows);
        Assert.Equal(2, model.Rows.Count);
        Assert.Equal("1", model.Rows[0].CardNumber);
        Assert.Equal("2", model.Rows[1].CardNumber);
    }

    [Fact]
    public async Task AddSetToCollection_ReturnsNotFound_ForInvalidProduct()
    {
        var result = await _controller.AddSetToCollection(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddSetToCollection_ShowsEmptyState_ForProductWithoutChecklist()
    {
        var product = await _context.Products.SingleAsync(p => p.DisplayName == "2024 Topps Chrome Black");

        var result = await _controller.AddSetToCollection(product.ProductId);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<CollectionBuilderViewModel>(view.Model);

        Assert.False(model.HasChecklistRows);
        Assert.Equal(0, model.TotalChecklistCards);
        Assert.Equal("This set exists, but its checklist has not been loaded.", model.EmptyMessage);
        Assert.Empty(model.Rows);
    }

    [Fact]
    public async Task Index_SearchFiltersByPlayerTeamAndCardNumber()
    {
        var product = await _context.Products.SingleAsync(p => p.DisplayName == "2022 Topps Chrome Black");
        var playerResult = await _controller.Index(product.ProductId, "Player One");
        var playerModel = Assert.IsType<ChecklistGridViewModel>(Assert.IsType<ViewResult>(playerResult).Model);
        Assert.Single(playerModel.Rows);
        Assert.Equal("Player One", playerModel.Rows[0].Player);

        var teamResult = await _controller.Index(product.ProductId, "Team B");
        var teamModel = Assert.IsType<ChecklistGridViewModel>(Assert.IsType<ViewResult>(teamResult).Model);
        Assert.Single(teamModel.Rows);
        Assert.Equal("Team B", teamModel.Rows[0].Team);

        var cardResult = await _controller.Index(product.ProductId, "2");
        var cardModel = Assert.IsType<ChecklistGridViewModel>(Assert.IsType<ViewResult>(cardResult).Model);
        Assert.Single(cardModel.Rows);
        Assert.Equal("2", cardModel.Rows[0].CardNumber);
    }

    [Fact]
    public async Task AddSetToCollection_IncrementsExistingOwnedQuantity_WhenSubmitted()
    {
        var product = await _context.Products.SingleAsync(p => p.DisplayName == "2022 Topps Chrome Black");
        var checklist = await _context.ChecklistItems.Where(x => x.ProductId == product.ProductId).ToListAsync();
        Assert.Equal(2, checklist.Count);

        var rowA = checklist[0];
        var rowB = checklist[1];
        _context.Cards.Add(new Card
        {
            ProductId = product.ProductId,
            ChecklistItemId = rowA.ChecklistItemId,
            Subject = rowA.Subject,
            Team = rowA.Team,
            Year = product.Year,
            Set = product.DisplayName,
            CardNumber = rowA.CardNumber,
            Quantity = 2,
            Category = "Baseball",
            ListingStatus = "Not Listed"
        });
        await _context.SaveChangesAsync();

        var model = new CollectionBuilderViewModel
        {
            ProductId = product.ProductId,
            ProductName = product.DisplayName,
            Rows = new List<CollectionBuilderRow>
            {
                new() { ChecklistItemId = rowA.ChecklistItemId, ProductId = product.ProductId, QuantityToAdd = 1, UseStockImage = true },
                new() { ChecklistItemId = rowB.ChecklistItemId, ProductId = product.ProductId, QuantityToAdd = 2, UseStockImage = true }
            }
        };

        var result = await _controller.AddSetToCollection(model);

        Assert.IsType<RedirectToActionResult>(result);

        var ownedA = await _context.Cards.Where(c => c.ChecklistItemId == rowA.ChecklistItemId && c.ProductId == product.ProductId).SumAsync(c => c.Quantity);
        var ownedB = await _context.Cards.Where(c => c.ChecklistItemId == rowB.ChecklistItemId && c.ProductId == product.ProductId).SumAsync(c => c.Quantity);

        Assert.Equal(3, ownedA);
        Assert.Equal(2, ownedB);
    }

    [Fact]
    public async Task Index_ExposesChecklistStatusAndCounters_ForLoadedChecklistProduct()
    {
        var product = await _context.Products.SingleAsync(p => p.DisplayName == "2022 Topps Chrome Black");

        var result = await _controller.Index(product.ProductId, null);

        var model = Assert.IsType<ChecklistGridViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal("Checklist partially loaded", model.ChecklistStatusLabel);
        Assert.Equal(2, model.TotalCount);
        Assert.Equal(2, model.CardsWithPlayers);
        Assert.Equal(2, model.CardsWithTeams);
        Assert.Equal(2, model.CardsWithReferenceImages);
        Assert.Equal(1, model.ChecklistSectionCount);
    }

    [Fact]
    public async Task Index_ExposesChecklistUnavailableStatus_ForShellProduct()
    {
        var product = await _context.Products.SingleAsync(p => p.DisplayName == "2024 Topps Chrome Black");

        var result = await _controller.Index(product.ProductId, null);

        var model = Assert.IsType<ChecklistGridViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal("Checklist unavailable", model.ChecklistStatusLabel);
        Assert.Equal(0, model.TotalCount);
        Assert.Empty(model.Rows);
    }

    [Fact]
    public async Task FindChecklistOnline_RedirectsWithPreparedQueryMessage()
    {
        var product = await _context.Products.SingleAsync(p => p.DisplayName == "2022 Topps Chrome Black");

        var result = await _controller.FindChecklistOnline(product.ProductId);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ChecklistsController.Index), redirect.ActionName);
        var message = Assert.IsType<string>(_controller.TempData["Success"]);
        Assert.Contains("Checklist research query prepared", message);
        Assert.Contains("checklist", message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("card numbers", message, StringComparison.OrdinalIgnoreCase);
    }

    private static void SeedData(ApplicationDbContext context)
    {
        var baseball = context.Sports.Single();
        var topps = context.Brands.Single(brand => brand.BrandName == "Topps");

        var productWithChecklist = new Product
        {
            ProductId = Guid.NewGuid(),
            Year = 2022,
            ProductName = "Chrome Black",
            DisplayName = "2022 Topps Chrome Black",
            SportId = baseball.SportId,
            BrandId = topps.BrandId,
            IsActive = true
        };

        var productWithoutChecklist = new Product
        {
            ProductId = Guid.NewGuid(),
            Year = 2024,
            ProductName = "Chrome Black",
            DisplayName = "2024 Topps Chrome Black",
            SportId = baseball.SportId,
            BrandId = topps.BrandId,
            IsActive = true
        };

        context.Products.AddRange(productWithChecklist, productWithoutChecklist);

        context.ChecklistItems.AddRange(
            new ChecklistItem
            {
                ChecklistItemId = Guid.NewGuid(),
                ProductId = productWithChecklist.ProductId,
                CardNumber = "1",
                Subject = "Player One",
                Team = "Team A",
                Subset = "Base",
                StockImageUrl = "https://example.com/one.jpg"
            },
            new ChecklistItem
            {
                ChecklistItemId = Guid.NewGuid(),
                ProductId = productWithChecklist.ProductId,
                CardNumber = "2",
                Subject = "Player Two",
                Team = "Team B",
                Subset = "Base",
                StockImageUrl = "https://example.com/two.jpg"
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

    private sealed class EmptyTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}

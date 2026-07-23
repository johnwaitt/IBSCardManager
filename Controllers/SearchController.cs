using IBSCardManager.Data;
using IBSCardManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Controllers;

public class SearchController : Controller
{
    private readonly ApplicationDbContext _context;
    public SearchController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Index(string? query, string? player, string? team, int? year, string? set, string? cardNumber)
    {
        var model = new CardSearchViewModel
        {
            Query = query?.Trim(), Player = player?.Trim(), Team = team?.Trim(), Year = year,
            Set = set?.Trim(), CardNumber = cardNumber?.Trim()
        };

        if (!HasSearch(model)) return View(model);

        var tokens = Tokenize(string.Join(' ', new[] { model.Query, model.Player, model.Team, model.Set, model.CardNumber }.Where(x => !string.IsNullOrWhiteSpace(x))));
        var inventory = await _context.Cards.AsNoTracking().Take(3000).ToListAsync();
        var checklist = await _context.ChecklistItems.AsNoTracking().Include(x => x.Product).Take(10000).ToListAsync();

        model.Results.AddRange(inventory.Select(card => new CardSearchResult
        {
            Source = "Inventory", RecordId = card.CardId, ProductId = card.ProductId, Player = card.Subject,
            Team = card.Team, Year = card.Year, Set = card.Set, CardNumber = card.CardNumber, Variety = card.Variety,
            ImageUrl = PreferredImage(card), Quantity = card.Quantity, IsRookie = card.IsRookie,
            IsAutograph = card.IsAutograph, IsRelic = card.IsRelic,
            Score = Score(tokens, model, card.Subject, card.Team, card.Year, card.Set, card.CardNumber, card.Variety)
        }).Where(x => x.Score > 0));

        model.Results.AddRange(checklist.Select(item => new CardSearchResult
        {
            Source = "Catalog", RecordId = item.ChecklistItemId, ProductId = item.ProductId, Player = item.Subject,
            Team = item.Team, Year = item.Product?.Year, Set = item.Product?.DisplayName, CardNumber = item.CardNumber,
            Variety = item.Subset, ImageUrl = item.StockImageUrl, Quantity = 0, IsRookie = item.IsRookie,
            IsAutograph = item.IsAutograph, IsRelic = item.IsRelic,
            Score = Score(tokens, model, item.Subject, item.Team, item.Product?.Year, item.Product?.DisplayName, item.CardNumber, item.Subset)
        }).Where(x => x.Score > 0));

        model.Results = model.Results.OrderByDescending(x => x.Score).ThenByDescending(x => x.Source == "Inventory").ThenBy(x => x.Player).Take(150).ToList();
        model.OnlineLinks = BuildOnlineLinks(BuildSearchText(model));
        return View(model);
    }

    private static bool HasSearch(CardSearchViewModel m) => !string.IsNullOrWhiteSpace(m.Query) || !string.IsNullOrWhiteSpace(m.Player) || !string.IsNullOrWhiteSpace(m.Team) || m.Year.HasValue || !string.IsNullOrWhiteSpace(m.Set) || !string.IsNullOrWhiteSpace(m.CardNumber);
    private static string[] Tokenize(string value) => value.Split(new[] { ' ', ',', '-', '/', '#', '(', ')' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(x => x.ToLowerInvariant()).Distinct().ToArray();
    private static string PreferredImage(IBSCardManager.Entities.Card card) => card.ImageSourcePreference == "Stock" && !string.IsNullOrWhiteSpace(card.StockImageUrl) ? card.StockImageUrl! : card.FrontImagePath ?? card.StockImageUrl ?? string.Empty;

    private static int Score(string[] tokens, CardSearchViewModel filter, string? player, string? team, int? year, string? set, string? number, string? variety)
    {
        var text = string.Join(' ', new[] { player, team, year?.ToString(), set, number, variety }.Where(x => !string.IsNullOrWhiteSpace(x))).ToLowerInvariant();
        var score = tokens.Sum(t => text.Contains(t) ? 12 : 0);
        if (!string.IsNullOrWhiteSpace(filter.Player) && Contains(player, filter.Player)) score += 35;
        if (!string.IsNullOrWhiteSpace(filter.Team) && Contains(team, filter.Team)) score += 20;
        if (filter.Year.HasValue && year == filter.Year) score += 25;
        if (!string.IsNullOrWhiteSpace(filter.Set) && Contains(set, filter.Set)) score += 30;
        if (!string.IsNullOrWhiteSpace(filter.CardNumber) && string.Equals(Normalize(number), Normalize(filter.CardNumber), StringComparison.OrdinalIgnoreCase)) score += 45;
        if (tokens.Length > 0 && tokens.All(t => text.Contains(t))) score += 30;
        return score;
    }
    private static bool Contains(string? source, string value) => source?.Contains(value, StringComparison.OrdinalIgnoreCase) == true;
    private static string Normalize(string? value) => (value ?? string.Empty).Trim().TrimStart('#').Replace(" ", string.Empty);
    private static string BuildSearchText(CardSearchViewModel m) => string.Join(' ', new[] { m.Year?.ToString(), m.Set, m.CardNumber, m.Player, m.Team, m.Query }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
    private static List<OnlineSearchLink> BuildOnlineLinks(string search)
    {
        var q = Uri.EscapeDataString(search);
        return new()
        {
            new() { Provider = "eBay", Description = "Search active marketplace listings", Url = $"https://www.ebay.com/sch/i.html?_nkw={q}&LH_Sold=0&LH_Complete=0" },
            new() { Provider = "eBay Sold", Description = "Compare completed and sold listings", Url = $"https://www.ebay.com/sch/i.html?_nkw={q}&LH_Sold=1&LH_Complete=1" },
            new() { Provider = "130 Point", Description = "Open sales-search provider", Url = "https://130point.com/sales/" },
            new() { Provider = "Trading Card Database", Description = "Search public checklist references", Url = $"https://www.google.com/search?q=site%3Atcdb.com+{q}" },
            new() { Provider = "Beckett", Description = "Search Beckett reference pages", Url = $"https://www.google.com/search?q=site%3Abeckett.com+{q}" }
        };
    }
}

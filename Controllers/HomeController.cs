using IBSCardManager.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalCards = await _context.Cards.SumAsync(card => card.Quantity);

        ViewBag.CollectionValue = await _context.Cards
            .SumAsync(card => (card.MyValue ?? 0) * card.Quantity);

        ViewBag.GradedCards = await _context.Cards
            .Where(card => card.GradeIssuer != null && card.GradeIssuer != "")
            .SumAsync(card => card.Quantity);

        ViewBag.ActiveListings = await _context.Cards
            .Where(card => card.ListingStatus == "Active")
            .SumAsync(card => card.Quantity);

        return View();
    }
}
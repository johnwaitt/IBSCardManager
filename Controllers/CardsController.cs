using IBSCardManager.Data;
using IBSCardManager.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Controllers
{
    public class CardsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CardsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var cardsQuery = _context.Cards
                .Include(card => card.Product)
                .ThenInclude(product => product!.Brand)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                cardsQuery = cardsQuery.Where(card =>
                    card.Subject.Contains(search) ||
                    (card.Team != null && card.Team.Contains(search)) ||
                    (card.Set != null && card.Set.Contains(search)) ||
                    (card.CardNumber != null && card.CardNumber.Contains(search)) ||
                    (card.CertNumber != null && card.CertNumber.Contains(search)));
            }

            var cards = await cardsQuery
                .OrderByDescending(card => card.Year)
                .ThenBy(card => card.Subject)
                .ToListAsync();

            var model = new IBSCardManager.Models.InventoryViewModel
            {
                Cards = cards,
                Search = search,
                RecordCount = cards.Count,
                PieceCount = cards.Sum(card => Math.Max(card.Quantity, 1)),
                CollectionValue = cards.Sum(card => (card.MyValue ?? 0m) * Math.Max(card.Quantity, 1)),
                GradedCount = cards.Count(card => !string.IsNullOrWhiteSpace(card.GradeIssuer)),
                ListedCount = cards.Count(card =>
                    !string.IsNullOrWhiteSpace(card.ListingStatus) &&
                    !string.Equals(card.ListingStatus, "Not Listed", StringComparison.OrdinalIgnoreCase)),
                Teams = cards
                    .Where(card => !string.IsNullOrWhiteSpace(card.Team))
                    .Select(card => card.Team!)
                    .Distinct()
                    .OrderBy(team => team)
                    .ToList(),
                Brands = cards
                    .Where(card => card.Product?.Brand?.BrandName != null)
                    .Select(card => card.Product!.Brand!.BrandName)
                    .Distinct()
                    .OrderBy(brand => brand)
                    .ToList(),
                Years = cards
                    .Where(card => card.Year.HasValue)
                    .Select(card => card.Year!.Value)
                    .Distinct()
                    .OrderByDescending(year => year)
                    .ToList(),
                Grades = cards
                    .Where(card => !string.IsNullOrWhiteSpace(card.GradeIssuer))
                    .Select(card => card.GradeIssuer!)
                    .Distinct()
                    .OrderBy(grade => grade)
                    .ToList(),
                Statuses = cards
                    .Where(card => !string.IsNullOrWhiteSpace(card.ListingStatus))
                    .Select(card => card.ListingStatus!)
                    .Distinct()
                    .OrderBy(status => status)
                    .ToList(),
                StorageBoxes = cards
                    .Where(card => !string.IsNullOrWhiteSpace(card.StorageBox))
                    .Select(card => card.StorageBox!)
                    .Distinct()
                    .OrderBy(box => box)
                    .ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> Details(
            Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var card = await _context.Cards
                .Include(item => item.Product)
                .ThenInclude(product => product!.Brand)
                .FirstOrDefaultAsync(item =>
                    item.CardId == id);

            if (card == null)
            {
                return NotFound();
            }

            return View(card);
        }

        public async Task<IActionResult> Create()
        {
            await LoadProductDropdownAsync();

            return View(new Card
            {
                Category = "Baseball",
                Quantity = 1,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "CardId,Subject,Team,Year,Set,ProductId,ChecklistItemId," +
                "CardNumber,Variety,Serial,Category," +
                "GradeIssuer,Grade,AutographGrade,CertNumber," +
                "IsRookie,IsAutograph,IsRelic,IsRefractor,StockImageUrl,ImageSourcePreference,Quantity," +
                "MyCost,PsaEstimate,MyValue,ListingPrice," +
                "ListingStatus,StorageBox,StorageRow,StorageBin," +
                "FrontImagePath,BackImagePath,MyNotes")]
            Card card)
        {
            card.Category = "Baseball";
            card.CreatedDate = DateTime.UtcNow;
            card.ModifiedDate = DateTime.UtcNow;

            await ApplySelectedProductAsync(card);

            if (ModelState.IsValid)
            {
                _context.Add(card);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await LoadProductDropdownAsync(card.ProductId);
            return View(card);
        }

        public async Task<IActionResult> Edit(
            Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var card = await _context.Cards.FindAsync(id);

            if (card == null)
            {
                return NotFound();
            }

            await LoadProductDropdownAsync(card.ProductId);
            return View(card);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id,
            [Bind(
                "CardId,Subject,Team,Year,Set,ProductId,ChecklistItemId," +
                "CardNumber,Variety,Serial,Category," +
                "GradeIssuer,Grade,AutographGrade,CertNumber," +
                "IsRookie,IsAutograph,IsRelic,IsRefractor,StockImageUrl,ImageSourcePreference,Quantity," +
                "MyCost,PsaEstimate,MyValue,ListingPrice," +
                "ListingStatus,StorageBox,StorageRow,StorageBin," +
                "FrontImagePath,BackImagePath,MyNotes," +
                "CreatedDate")]
            Card card)
        {
            if (id != card.CardId)
            {
                return NotFound();
            }

            card.Category = "Baseball";
            card.ModifiedDate = DateTime.UtcNow;

            await ApplySelectedProductAsync(card);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(card);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CardExists(card.CardId))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await LoadProductDropdownAsync(card.ProductId);
            return View(card);
        }

        public async Task<IActionResult> Delete(
            Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var card = await _context.Cards
                .Include(item => item.Product)
                .ThenInclude(product => product!.Brand)
                .FirstOrDefaultAsync(item =>
                    item.CardId == id);

            if (card == null)
            {
                return NotFound();
            }

            return View(card);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            Guid id)
        {
            var card = await _context.Cards.FindAsync(id);

            if (card != null)
            {
                _context.Cards.Remove(card);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }



        [HttpGet]
        public async Task<IActionResult> ExportCsv()
        {
            var cards = await _context.Cards
                .Include(card => card.Product)
                .ThenInclude(product => product!.Brand)
                .AsNoTracking()
                .OrderByDescending(card => card.Year)
                .ThenBy(card => card.Subject)
                .ToListAsync();

            static string Csv(string? value)
            {
                value ??= string.Empty;
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            var lines = new List<string>
            {
                "Player,Team,Year,Brand,Set,Card Number,Variety,Serial,Grade,Quantity,Value,Listing Status,Storage Box,Storage Row,Storage Bin"
            };

            lines.AddRange(cards.Select(card => string.Join(",", new[]
            {
                Csv(card.Subject), Csv(card.Team), card.Year?.ToString() ?? string.Empty,
                Csv(card.Product?.Brand?.BrandName), Csv(card.Set ?? card.Product?.DisplayName),
                Csv(card.CardNumber), Csv(card.Variety), Csv(card.Serial),
                Csv(string.IsNullOrWhiteSpace(card.GradeIssuer) ? "Raw" : $"{card.GradeIssuer} {card.Grade}".Trim()),
                Math.Max(card.Quantity, 1).ToString(), (card.MyValue ?? 0m).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                Csv(card.ListingStatus), Csv(card.StorageBox), Csv(card.StorageRow), Csv(card.StorageBin)
            })));

            return File(System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines)), "text/csv", $"card-inventory-{DateTime.Now:yyyy-MM-dd}.csv");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(string selectedIds)
        {
            var ids = (selectedIds ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(value => Guid.TryParse(value, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                TempData["InventoryMessage"] = "No cards were selected.";
                return RedirectToAction(nameof(Index));
            }

            var cards = await _context.Cards.Where(card => ids.Contains(card.CardId)).ToListAsync();
            _context.Cards.RemoveRange(cards);
            await _context.SaveChangesAsync();
            TempData["InventoryMessage"] = $"Deleted {cards.Count} card{(cards.Count == 1 ? string.Empty : "s")}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ChecklistLookup(Guid productId, string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber)) return BadRequest();
            var item = await _context.ChecklistItems.AsNoTracking()
                .Where(x => x.ProductId == productId && x.CardNumber == cardNumber.Trim())
                .OrderBy(x => x.Subject)
                .FirstOrDefaultAsync();
            if (item == null) return NotFound();
            return Json(new { item.ChecklistItemId, item.Subject, item.Team, item.Subset, item.IsRookie, item.IsAutograph, item.IsRelic, item.IsRefractor, item.StockImageUrl });
        }

        private async Task LoadProductDropdownAsync(
            Guid? selectedProductId = null)
        {
            var products = await _context.Products
                .Include(product => product.Brand)
                .Where(product =>
                    product.IsActive &&
                    product.Sport != null &&
                    product.Sport.SportName == "Baseball")
                .OrderByDescending(product => product.Year)
                .ThenBy(product => product.Brand!.BrandName)
                .ThenBy(product => product.ProductName)
                .ToListAsync();

            ViewBag.ProductOptions = new SelectList(
                products,
                nameof(Product.ProductId),
                nameof(Product.DisplayName),
                selectedProductId);
        }

        private async Task ApplySelectedProductAsync(
            Card card)
        {
            if (card.ProductId == null)
            {
                return;
            }

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(item =>
                    item.ProductId == card.ProductId);

            if (product == null)
            {
                ModelState.AddModelError(
                    nameof(card.ProductId),
                    "The selected product was not found.");

                return;
            }

            card.Year = product.Year;
            card.Set = product.DisplayName;
        }

        private bool CardExists(Guid id)
        {
            return _context.Cards.Any(card =>
                card.CardId == id);
        }
    }
}
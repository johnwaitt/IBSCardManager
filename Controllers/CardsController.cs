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

        public async Task<IActionResult> Index(
            string? search)
        {
            ViewBag.Search = search;

            var cards = _context.Cards
                .Include(card => card.Product)
                .ThenInclude(product => product!.Brand)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                cards = cards.Where(card =>
                    card.Subject.Contains(search) ||
                    (card.Team != null &&
                     card.Team.Contains(search)) ||
                    (card.Set != null &&
                     card.Set.Contains(search)) ||
                    (card.CardNumber != null &&
                     card.CardNumber.Contains(search)) ||
                    (card.CertNumber != null &&
                     card.CertNumber.Contains(search)));
            }

            var results = await cards
                .OrderByDescending(card => card.Year)
                .ThenBy(card => card.Subject)
                .ToListAsync();

            return View(results);
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
                "CardId,Subject,Team,Year,Set,ProductId," +
                "CardNumber,Variety,Serial,Category," +
                "GradeIssuer,Grade,AutographGrade,CertNumber," +
                "IsRookie,IsAutograph,IsRelic,Quantity," +
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
                "CardId,Subject,Team,Year,Set,ProductId," +
                "CardNumber,Variety,Serial,Category," +
                "GradeIssuer,Grade,AutographGrade,CertNumber," +
                "IsRookie,IsAutograph,IsRelic,Quantity," +
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
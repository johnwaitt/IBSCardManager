using System.Text;
using ClosedXML.Excel;
using IBSCardManager.Data;
using IBSCardManager.Data.Catalog;
using IBSCardManager.Entities;
using IBSCardManager.Models;
using IBSCardManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Controllers;

public class ChecklistsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ICardWebSearchService _cardWebSearchService;
    private readonly IChecklistCandidateService _checklistCandidateService;

    public ChecklistsController(
        ApplicationDbContext context,
        IWebHostEnvironment environment,
        ICardWebSearchService cardWebSearchService,
        IChecklistCandidateService checklistCandidateService)
    {
        _context = context;
        _environment = environment;
        _cardWebSearchService = cardWebSearchService;
        _checklistCandidateService = checklistCandidateService;
    }

    public async Task<IActionResult> Index(Guid? productId, string? search)
    {
        var products = await _context.Products
            .Include(product => product.Brand)
            .OrderByDescending(product => product.Year)
            .ThenBy(product => product.DisplayName)
            .ToListAsync();

        var productStats = await _context.ChecklistItems
            .AsNoTracking()
            .GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                ChecklistCardCount = group.Count(),
                ChecklistSectionCount = group.Select(item => item.Subset).Where(value => value != null && value != "").Distinct().Count(),
                PlayerLinkCount = group.Count(item => item.Subject != null && item.Subject != ""),
                TeamLinkCount = group.Count(item => item.Team != null && item.Team != ""),
                ReferenceImageCount = group.Count(item => item.ReferenceImageUrl != null && item.ReferenceImageUrl != "")
            })
            .ToListAsync();

        var statMap = productStats.ToDictionary(stat => stat.ProductId);

        ViewBag.Products = new SelectList(products, "ProductId", "DisplayName", productId);
        ViewBag.ProductId = productId;
        ViewBag.Search = search;

        var model = new ChecklistGridViewModel
        {
            ProductId = productId,
            Search = search,
            Products = products.Select(product =>
            {
                statMap.TryGetValue(product.ProductId, out var stat);
                var status = ResolveChecklistStatus(
                    stat?.ChecklistCardCount ?? 0,
                    stat?.ChecklistSectionCount ?? 0,
                    hasSourceProvenance: false,
                    importFailed: false,
                    requiresReview: false);

                return new ChecklistProductOptionViewModel
                {
                    ProductId = product.ProductId,
                    DisplayName = product.DisplayName,
                    ChecklistStatusLabel = status.ToLabel(),
                    ChecklistCardCount = stat?.ChecklistCardCount ?? 0,
                    ChecklistSectionCount = stat?.ChecklistSectionCount ?? 0,
                    PlayerLinkCount = stat?.PlayerLinkCount ?? 0,
                    TeamLinkCount = stat?.TeamLinkCount ?? 0,
                    ReferenceImageCount = stat?.ReferenceImageCount ?? 0,
                    LastImportSource = product.LastChecklistImportSource
                };
            }).ToList()
        };

        if (!productId.HasValue)
        {
            model.EmptyMessage = "Select a set to view its checklist.";
            return View(model);
        }

        var selectedProduct = products.FirstOrDefault(product => product.ProductId == productId.Value);
        if (selectedProduct == null)
        {
            model.ErrorMessage = "The selected set could not be found.";
            return View(model);
        }

        model.ProductName = selectedProduct.DisplayName;
        model.LastImportSource = selectedProduct.LastChecklistImportSource;

        var query = _context.ChecklistItems
            .AsNoTracking()
            .Where(item => item.ProductId == productId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(item =>
                item.CardNumber.Contains(term) ||
                item.Subject.Contains(term) ||
                (item.Team != null && item.Team.Contains(term)) ||
                (item.Subset != null && item.Subset.Contains(term)) ||
                (item.Parallel != null && item.Parallel.Contains(term)) ||
                (item.Variation != null && item.Variation.Contains(term)));
        }

        var checklistItems = await query
            .OrderBy(item => item.Subset)
            .ThenBy(item => item.CardNumber)
            .ThenBy(item => item.Subject)
            .Take(1000)
            .ToListAsync();

        var rows = checklistItems
            .Select(item =>
            {
                int? serialMaximum = null;
                var serialSource = item.PrintRun ?? item.SerialNumber;
                if (int.TryParse(serialSource, out var parsedSerial))
                {
                    serialMaximum = parsedSerial;
                }

                return new ChecklistGridRowViewModel
                {
                    ChecklistItemId = item.ChecklistItemId,
                    ProductId = item.ProductId,
                    ProductDisplayName = selectedProduct.DisplayName,
                    CardNumber = item.CardNumber,
                    Player = item.Subject,
                    Team = item.Team,
                    CardType = item.Subset,
                    ChecklistSection = item.Subset,
                    Parallel = item.Parallel,
                    Variation = item.Variation,
                    IsRookie = item.IsRookie,
                    IsAutograph = item.IsAutograph,
                    IsRelic = item.IsRelic,
                    SerialMaximum = serialMaximum,
                    OwnedQuantity = 0,
                    ReferenceImageUrl = string.IsNullOrWhiteSpace(item.ReferenceImageUrl) ? item.StockImageUrl : item.ReferenceImageUrl
                };
            })
            .ToList();

        var totalCount = await _context.ChecklistItems.CountAsync(item => item.ProductId == productId.Value);
        var sections = await _context.ChecklistItems
            .AsNoTracking()
            .Where(item => item.ProductId == productId.Value && item.Subset != null && item.Subset != "")
            .Select(item => item.Subset)
            .Distinct()
            .CountAsync();

        var cardsWithPlayers = await _context.ChecklistItems.CountAsync(item => item.ProductId == productId.Value && item.Subject != null && item.Subject != "");
        var cardsWithTeams = await _context.ChecklistItems.CountAsync(item => item.ProductId == productId.Value && item.Team != null && item.Team != "");
        var cardsWithImages = await _context.ChecklistItems.CountAsync(item => item.ProductId == productId.Value && ((item.ReferenceImageUrl != null && item.ReferenceImageUrl != "") || (item.StockImageUrl != null && item.StockImageUrl != "")));

        var checklistStatus = ResolveChecklistStatus(
            totalCount,
            sections,
            hasSourceProvenance: false,
            importFailed: false,
            requiresReview: false);

        model.ChecklistStatusLabel = checklistStatus.ToLabel();
        if (rows.Count == 0)
        {
            model.EmptyMessage = "This set exists, but its checklist has not been loaded.";
        }

        model.Rows = rows;
        model.TotalCount = totalCount;
        model.FilteredCount = rows.Count;
        model.CardsWithPlayers = cardsWithPlayers;
        model.CardsWithTeams = cardsWithTeams;
        model.CardsWithReferenceImages = cardsWithImages;
        model.ChecklistSectionCount = sections;

        return View(model);
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Load2025ToppsChromeBlackBase()
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Year == 2025 && p.ProductName == "Chrome Black");
        if (product == null)
        {
            TempData["Success"] = "The 2025 Topps Chrome Black product was not found. Run the application once so the product seeder can create it.";
            return RedirectToAction(nameof(Index));
        }

        var existing = await _context.ChecklistItems.Where(x => x.ProductId == product.ProductId).ToListAsync();
        var added = 0;
        var updated = 0;
        foreach (var source in ToppsChromeBlack2025Base.Cards)
        {
            var item = existing.FirstOrDefault(x => x.CardNumber == source.Number && x.Subject == source.Subject);
            if (item == null)
            {
                item = new ChecklistItem
                {
                    ProductId = product.ProductId,
                    CardNumber = source.Number,
                    Subject = source.Subject,
                    Team = source.Team,
                    IsRookie = source.Rookie,
                    Subset = "Base"
                };
                _context.ChecklistItems.Add(item);
                existing.Add(item);
                added++;
            }
            else
            {
                item.Team = source.Team;
                item.IsRookie = source.Rookie;
                item.Subset ??= "Base";
                updated++;
            }
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = $"2025 Topps Chrome Black base checklist loaded: {added} added, {updated} updated.";
        return RedirectToAction(nameof(AddSetToCollection), new { productId = product.ProductId });
    }

    public async Task<IActionResult> Create(Guid? productId) { await LoadProducts(productId); return View(new ChecklistItem { ProductId = productId ?? Guid.Empty }); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ChecklistItem item)
    {
        if (!ModelState.IsValid) { await LoadProducts(item.ProductId); return View(item); }
        _context.Add(item); await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { productId = item.ProductId });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var item = await _context.ChecklistItems.FindAsync(id); if (item == null) return NotFound();
        await LoadProducts(item.ProductId); return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ChecklistItem item)
    {
        if (id != item.ChecklistItemId) return NotFound();
        if (!ModelState.IsValid) { await LoadProducts(item.ProductId); return View(item); }
        _context.Update(item); await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { productId = item.ProductId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.ChecklistItems.FindAsync(id); if (item == null) return NotFound();
        var productId = item.ProductId;
        var cards = await _context.Cards.Where(c => c.ChecklistItemId == id).ToListAsync();
        foreach (var card in cards) card.ChecklistItemId = null;
        _context.Remove(item); await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { productId });
    }

    public async Task<IActionResult> Import(Guid? productId) { await LoadProducts(productId); return View(); }

    [HttpGet]
    public async Task<IActionResult> FindChecklistOnline(Guid? productId)
    {
        if (!productId.HasValue)
        {
            TempData["Error"] = "Select a set before searching for checklist sources.";
            return RedirectToAction(nameof(Index));
        }

        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.Sport)
            .FirstOrDefaultAsync(p => p.ProductId == productId.Value);

        if (product == null)
        {
            return NotFound();
        }

        var query = BuildChecklistSearchQuery(product);
        var request = new WebChecklistSearchRequest
        {
            ProductId = product.ProductId,
            Year = product.Year,
            Manufacturer = product.Brand?.BrandName,
            Brand = product.Brand?.BrandName,
            Product = product.ProductName,
            Edition = product.ProductName,
            Sport = product.Sport?.SportName
        };

        var candidates = await _cardWebSearchService.SearchChecklistAsync(request);

        foreach (var candidate in candidates)
        {
            _context.WebSearchResults.Add(new WebSearchResult
            {
                ProductId = product.ProductId,
                SearchScope = "Checklist",
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

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Checklist research query prepared: {query}";
        return RedirectToAction(nameof(Index), new { productId = product.ProductId });
    }

    [HttpGet]
    public IActionResult PasteChecklist(Guid? productId)
    {
        TempData["Success"] = "Paste checklist workflow foundation is enabled. Paste/import preview and field mapping are required before final import.";
        return RedirectToAction(nameof(Import), new { productId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(Guid productId, IFormFile checklistFile, List<IFormFile>? stockImages)
    {
        await LoadProducts(productId);
        if (checklistFile == null || checklistFile.Length == 0) { ModelState.AddModelError("checklistFile", "Choose a CSV or Excel file."); return View(); }
        if (!await _context.Products.AnyAsync(p => p.ProductId == productId)) { ModelState.AddModelError("productId", "Select a valid set."); return View(); }
        var rows = await ReadChecklistRowsAsync(checklistFile);
        if (rows.Count == 0) { ModelState.AddModelError("checklistFile", "The checklist file is empty."); return View(); }
        var header = rows[0].Select(x => x.Trim().ToLowerInvariant()).ToList();
        int Find(params string[] names) { foreach (var n in names) { var i = header.IndexOf(n); if (i >= 0) return i; } return -1; }
        var numberIndex = Find("cardnumber", "card number", "card #", "number"); var subjectIndex = Find("subject", "player", "player name", "name");
        if (numberIndex < 0 || subjectIndex < 0) { ModelState.AddModelError("checklistFile", "The file must include Card Number and Player/Subject columns."); return View(); }
        var teamIndex = Find("team"); var subsetIndex = Find("subset", "insert", "variation"); var rookieIndex = Find("isrookie", "rookie", "rc"); var autoIndex = Find("isautograph", "autograph", "auto"); var relicIndex = Find("isrelic", "relic"); var refractorIndex = Find("isrefractor", "refractor"); var imageIndex = Find("stockimageurl", "stock image", "image");
        var uploadedImages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (stockImages is { Count: > 0 })
        {
            var relativeFolder = $"uploads/checklists/{productId:N}";
            var physicalFolder = Path.Combine(_environment.WebRootPath, relativeFolder.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(physicalFolder);
            foreach (var file in stockImages.Where(f => f.Length > 0))
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (extension is not (".jpg" or ".jpeg" or ".png" or ".webp")) continue;
                var stem = Path.GetFileNameWithoutExtension(file.FileName);
                var safeStem = string.Concat(stem.Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-'));
                var savedName = $"{safeStem}{extension}";
                await using var output = System.IO.File.Create(Path.Combine(physicalFolder, savedName));
                await file.CopyToAsync(output);
                uploadedImages[NormalizeImageKey(stem)] = $"/{relativeFolder}/{savedName}";
            }
        }

        var sourceName = string.IsNullOrWhiteSpace(Request.Form["sourceName"]) ? "User Import" : Request.Form["sourceName"].ToString().Trim();
        var sourceType = string.IsNullOrWhiteSpace(Request.Form["sourceType"]) ? "UserUpload" : Request.Form["sourceType"].ToString().Trim();
        var sourceUrl = Null(Request.Form["sourceUrl"].ToString());
        var sourceProductIdentifier = Null(Request.Form["sourceProductIdentifier"].ToString());
        var sourceVersion = Null(Request.Form["sourceVersion"].ToString());
        var sourceLicenseUsageNotes = Null(Request.Form["sourceLicenseUsageNotes"].ToString());
        var importProfile = Null(Request.Form["importProfile"].ToString());
        var verificationStatus = string.IsNullOrWhiteSpace(Request.Form["verificationStatus"]) ? "Unverified" : Request.Form["verificationStatus"].ToString().Trim();
        var nowUtc = DateTime.UtcNow;

        var existing = await _context.ChecklistItems.Where(x => x.ProductId == productId).ToListAsync(); int added = 0, updated = 0;
        foreach (var rowWithIndex in rows.Skip(1).Select((values, index) => new { Values = values, RowNumber = index + 2 }).Where(x => x.Values.Any(value => !string.IsNullOrWhiteSpace(value))))
        {
            var v = rowWithIndex.Values;
            string Get(int i) => i >= 0 && i < v.Count ? v[i].Trim() : ""; bool Flag(int i) { var s = Get(i); return s.Equals("true", StringComparison.OrdinalIgnoreCase) || s.Equals("yes", StringComparison.OrdinalIgnoreCase) || s == "1" || s.Equals("x", StringComparison.OrdinalIgnoreCase); }
            var number = Get(numberIndex); var subject = Get(subjectIndex); if (number == "" || subject == "") continue;
            var item = existing.FirstOrDefault(x => x.CardNumber.Equals(number, StringComparison.OrdinalIgnoreCase) && x.Subject.Equals(subject, StringComparison.OrdinalIgnoreCase));
            if (item == null) { item = new ChecklistItem { ProductId = productId, CardNumber = number, Subject = subject }; _context.Add(item); existing.Add(item); added++; } else updated++;
            item.Team = Null(Get(teamIndex)); item.Subset = Null(Get(subsetIndex)); item.IsRookie = Flag(rookieIndex); item.IsAutograph = Flag(autoIndex); item.IsRelic = Flag(relicIndex); item.IsRefractor = Flag(refractorIndex); item.StockImageUrl = Null(Get(imageIndex)); if (string.IsNullOrWhiteSpace(item.StockImageUrl) && uploadedImages.TryGetValue(NormalizeImageKey(number), out var uploadedUrl)) item.StockImageUrl = uploadedUrl;
            item.SourceName = sourceName;
            item.SourceType = sourceType;
            item.SourceUrl = sourceUrl;
            item.SourceFile = checklistFile.FileName;
            item.SourceProductIdentifier = sourceProductIdentifier;
            item.SourceCardIdentifier = number;
            item.SourceDateImportedUtc = nowUtc;
            item.SourceVersion = sourceVersion;
            item.SourceLicenseUsageNotes = sourceLicenseUsageNotes;
            item.ImportProfile = importProfile;
            item.SourceOriginalRowNumber = rowWithIndex.RowNumber;
            item.SourceRawValuesJson = System.Text.Json.JsonSerializer.Serialize(v);
            item.SourceVerificationStatus = verificationStatus;
            if (!string.IsNullOrWhiteSpace(item.StockImageUrl) && string.IsNullOrWhiteSpace(item.ReferenceImageUrl))
            {
                item.ReferenceImageUrl = item.StockImageUrl;
                item.ReferenceImageSource ??= sourceName;
                item.ReferenceImageDateLocatedUtc ??= nowUtc;
                item.ReferenceImageUsageStatus ??= "Unknown";
                item.ReferenceImageVerificationStatus ??= "Unverified";
            }
        }

        var importedProduct = await _context.Products.FirstAsync(product => product.ProductId == productId);
        importedProduct.LastChecklistImportSource = sourceName;
        importedProduct.ChecklistLastImportedUtc = nowUtc;

        var importedRows = await _context.ChecklistItems.CountAsync(item => item.ProductId == productId);
        var importedSections = await _context.ChecklistItems.Where(item => item.ProductId == productId && item.Subset != null && item.Subset != "").Select(item => item.Subset).Distinct().CountAsync();
        importedProduct.ChecklistAvailabilityStatus = ResolveChecklistStatus(importedRows, importedSections, hasSourceProvenance: true, importFailed: false, requiresReview: false).ToLabel();

        _context.ChecklistImportHistories.Add(new ChecklistImportHistory
        {
            ProductId = productId,
            SourceName = sourceName,
            SourceType = sourceType,
            SourceUrl = sourceUrl,
            SourceFile = checklistFile.FileName,
            SourceProductIdentifier = sourceProductIdentifier,
            SourceVersion = sourceVersion,
            LicenseUsageNotes = sourceLicenseUsageNotes,
            ImportProfile = importProfile,
            VerificationStatus = verificationStatus,
            RetrievedUtc = nowUtc,
            ImportedUtc = nowUtc,
            RowsRead = Math.Max(0, rows.Count - 1),
            RowsImported = added,
            RowsUpdated = updated,
            Notes = "User-initiated checklist import"
        });

        await _context.SaveChangesAsync(); TempData["Success"] = $"Checklist import complete: {added} added, {updated} updated.";
        return RedirectToAction(nameof(Index), new { productId });
    }


    [HttpGet]
    public async Task<IActionResult> AddSetToCollection(Guid productId)
    {
        var product = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Sport)
            .FirstOrDefaultAsync(p => p.ProductId == productId);
        if (product == null) return NotFound();

        var items = await _context.ChecklistItems
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .OrderBy(x => x.CardNumber)
            .ThenBy(x => x.Subject)
            .ToListAsync();

        var existing = await _context.Cards
            .AsNoTracking()
            .Where(c => c.ProductId == productId && c.ChecklistItemId != null)
            .GroupBy(c => c.ChecklistItemId!.Value)
            .Select(g => new { ChecklistItemId = g.Key, Quantity = g.Sum(c => c.Quantity) })
            .ToDictionaryAsync(x => x.ChecklistItemId, x => x.Quantity);

        var totalChecklistCards = items.Count;
        var totalPiecesOwned = existing.Values.Sum();
        var uniqueOwned = existing.Count;
        var missingCount = Math.Max(0, totalChecklistCards - uniqueOwned);
        var completionPercent = totalChecklistCards > 0 ? Math.Round(uniqueOwned * 100m / totalChecklistCards, 1) : 0m;
        var checklistSectionCount = items.Where(item => !string.IsNullOrWhiteSpace(item.Subset)).Select(item => item.Subset).Distinct().Count();
        var checklistStatus = ResolveChecklistStatus(
            totalChecklistCards,
            checklistSectionCount,
            hasSourceProvenance: false,
            importFailed: false,
            requiresReview: false);

        var model = new CollectionBuilderViewModel
        {
            ProductId = productId,
            ProductName = product.DisplayName,
            ProductEdition = product.ProductName,
            Year = product.Year,
            Manufacturer = product.Brand?.BrandName,
            Brand = product.Brand?.BrandName,
            TotalChecklistCards = totalChecklistCards,
            UniqueOwned = uniqueOwned,
            TotalPiecesOwned = totalPiecesOwned,
            MissingCount = missingCount,
            CompletionPercent = completionPercent,
            HasChecklistRows = totalChecklistCards > 0,
            ChecklistStatusLabel = checklistStatus.ToLabel(),
            LastImportSource = product.LastChecklistImportSource,
            EmptyMessage = totalChecklistCards == 0 ? "This set exists, but its checklist has not been loaded." : null,
            Rows = items.Select(item => new CollectionBuilderRow
            {
                ChecklistItemId = item.ChecklistItemId,
                ProductId = product.ProductId,
                ChecklistSectionId = null,
                CardNumber = item.CardNumber,
                Subject = item.Subject,
                Team = item.Team,
                TeamSummary = item.Team ?? item.Subject,
                ChecklistSection = item.Subset,
                Parallel = item.Parallel,
                Variation = item.Variation,
                IsRookie = item.IsRookie,
                IsAutograph = item.IsAutograph,
                IsRelic = item.IsRelic,
                ReferenceImageUrl = item.StockImageUrl,
                StockImageUrl = item.StockImageUrl,
                ImageChoice = string.IsNullOrWhiteSpace(item.StockImageUrl) ? "No image" : "Use reference image",
                ExistingQuantity = existing.GetValueOrDefault(item.ChecklistItemId),
                QuantityToAdd = 0,
                UseStockImage = true,
                UseReferenceImage = true
            }).ToList()
        };

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSetToCollection(CollectionBuilderViewModel model)
    {
        var product = await _context.Products
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.ProductId == model.ProductId);
        if (product == null) return NotFound();

        var requested = model.Rows.Where(r => r.QuantityToAdd > 0).ToList();
        if (requested.Count == 0)
        {
            TempData["Success"] = "No quantities were entered.";
            return RedirectToAction(nameof(AddSetToCollection), new { productId = model.ProductId });
        }

        var ids = requested.Select(r => r.ChecklistItemId).ToList();
        var checklist = await _context.ChecklistItems.Where(x => ids.Contains(x.ChecklistItemId)).ToDictionaryAsync(x => x.ChecklistItemId);
        var existingCards = await _context.Cards.Where(c => c.ProductId == model.ProductId && c.ChecklistItemId != null && ids.Contains(c.ChecklistItemId.Value)).ToListAsync();

        var added = 0;
        var updated = 0;
        var submittedUniqueCount = requested.Count;
        var submittedPieces = requested.Sum(r => r.QuantityToAdd);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var row in requested)
            {
                if (!checklist.TryGetValue(row.ChecklistItemId, out var item)) continue;
                if (!string.IsNullOrWhiteSpace(row.StockImageUrl)) item.StockImageUrl = row.StockImageUrl.Trim();
                var card = existingCards.FirstOrDefault(c => c.ChecklistItemId == item.ChecklistItemId && string.IsNullOrWhiteSpace(c.GradeIssuer));
                if (card == null)
                {
                    card = new Card
                    {
                        ProductId = product.ProductId,
                        ChecklistItemId = item.ChecklistItemId,
                        Subject = item.Subject,
                        Team = item.Team,
                        Year = product.Year,
                        Set = product.DisplayName,
                        CardNumber = item.CardNumber,
                        Variety = item.Subset,
                        Category = "Baseball",
                        IsRookie = item.IsRookie,
                        IsAutograph = item.IsAutograph,
                        IsRelic = item.IsRelic,
                        IsRefractor = item.IsRefractor,
                        Quantity = row.QuantityToAdd,
                        StockImageUrl = item.StockImageUrl,
                        ImageSourcePreference = row.UseStockImage && !string.IsNullOrWhiteSpace(item.StockImageUrl) ? "Stock" : "Scan",
                        ListingStatus = "Not Listed",
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };
                    _context.Cards.Add(card);
                    added++;
                }
                else
                {
                    card.Quantity += row.QuantityToAdd;
                    if (row.UseStockImage && !string.IsNullOrWhiteSpace(item.StockImageUrl))
                    {
                        card.StockImageUrl = item.StockImageUrl;
                        if (string.IsNullOrWhiteSpace(card.FrontImagePath)) card.ImageSourcePreference = "Stock";
                    }
                    card.ModifiedDate = DateTime.Now;
                    updated++;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["InventoryMessage"] = $"Set import complete: {submittedUniqueCount} unique checklist cards and {submittedPieces} total pieces submitted; {added} inventory records added, {updated} quantities updated.";
            return RedirectToAction(nameof(AddSetToCollection), new { productId = model.ProductId });
        }
        catch
        {
            try
            {
                await transaction.RollbackAsync();
            }
            catch
            {
            }

            TempData["Error"] = "Set import failed. No inventory was changed.";
            return RedirectToAction(nameof(AddSetToCollection), new { productId = model.ProductId });
        }
    }

    public async Task<IActionResult> EditSet(Guid productId)
    {
        var product = await _context.Products
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.ProductId == productId);
        if (product == null) return NotFound();

        var items = await _context.ChecklistItems
            .Where(x => x.ProductId == productId)
            .OrderBy(x => x.CardNumber)
            .ThenBy(x => x.Subject)
            .ToListAsync();

        var cards = await _context.Cards
            .Where(c => c.ProductId == productId && c.ChecklistItemId != null)
            .ToListAsync();

        var model = new SetEditorViewModel
        {
            ProductId = productId,
            ProductName = product.DisplayName,
            Rows = items.Select(item =>
            {
                var card = cards.FirstOrDefault(c => c.ChecklistItemId == item.ChecklistItemId && string.IsNullOrWhiteSpace(c.GradeIssuer))
                    ?? cards.FirstOrDefault(c => c.ChecklistItemId == item.ChecklistItemId);
                return new SetEditorRow
                {
                    ChecklistItemId = item.ChecklistItemId,
                    CardNumber = item.CardNumber,
                    Subject = item.Subject,
                    Team = item.Team,
                    Position = item.Position,
                    Subset = item.Subset,
                    Parallel = item.Parallel,
                    Variation = item.Variation,
                    SerialNumber = item.SerialNumber,
                    PrintRun = item.PrintRun,
                    IsRookie = item.IsRookie,
                    IsAutograph = item.IsAutograph,
                    IsRelic = item.IsRelic,
                    IsRefractor = item.IsRefractor,
                    StockImageUrl = item.StockImageUrl,
                    StockBackImageUrl = item.StockBackImageUrl,
                    Quantity = card?.Quantity ?? 0,
                    ListingPrice = card?.ListingPrice,
                    GradeIssuer = card?.GradeIssuer,
                    Grade = card?.Grade,
                    CertNumber = card?.CertNumber,
                    EbaySku = card?.EbaySku,
                    EbayTitle = card?.EbayTitle,
                    EbayDescription = card?.EbayDescription,
                    EbayCategoryId = card?.EbayCategoryId,
                    EbayCondition = card?.EbayCondition,
                    ListingFormat = card?.ListingFormat ?? "FixedPrice",
                    BestOfferEnabled = card?.BestOfferEnabled ?? false,
                    ShippingPolicyName = card?.ShippingPolicyName,
                    ReturnPolicyName = card?.ReturnPolicyName,
                    PaymentPolicyName = card?.PaymentPolicyName,
                    PackageWeightOz = card?.PackageWeightOz,
                    PackageLengthIn = card?.PackageLengthIn,
                    PackageWidthIn = card?.PackageWidthIn,
                    PackageHeightIn = card?.PackageHeightIn
                };
            }).ToList()
        };

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSet(SetEditorViewModel model)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == model.ProductId);
        if (product == null) return NotFound();

        var ids = model.Rows.Select(r => r.ChecklistItemId).ToList();
        var items = await _context.ChecklistItems
            .Where(x => x.ProductId == model.ProductId && ids.Contains(x.ChecklistItemId))
            .ToDictionaryAsync(x => x.ChecklistItemId);
        var cards = await _context.Cards
            .Where(c => c.ProductId == model.ProductId && c.ChecklistItemId != null && ids.Contains(c.ChecklistItemId.Value))
            .ToListAsync();

        var updatedRows = 0;
        foreach (var row in model.Rows)
        {
            if (!items.TryGetValue(row.ChecklistItemId, out var item)) continue;

            item.CardNumber = row.CardNumber.Trim();
            item.Subject = row.Subject.Trim();
            item.Team = Null(row.Team ?? string.Empty);
            item.Position = Null(row.Position ?? string.Empty);
            item.Subset = Null(row.Subset ?? string.Empty);
            item.Parallel = Null(row.Parallel ?? string.Empty);
            item.Variation = Null(row.Variation ?? string.Empty);
            item.SerialNumber = Null(row.SerialNumber ?? string.Empty);
            item.PrintRun = Null(row.PrintRun ?? string.Empty);
            item.IsRookie = row.IsRookie;
            item.IsAutograph = row.IsAutograph;
            item.IsRelic = row.IsRelic;
            item.IsRefractor = row.IsRefractor;
            item.StockImageUrl = Null(row.StockImageUrl ?? string.Empty);
            item.StockBackImageUrl = Null(row.StockBackImageUrl ?? string.Empty);

            var card = cards.FirstOrDefault(c => c.ChecklistItemId == row.ChecklistItemId && string.IsNullOrWhiteSpace(c.GradeIssuer))
                ?? cards.FirstOrDefault(c => c.ChecklistItemId == row.ChecklistItemId);

            if (row.Quantity > 0 && card == null)
            {
                card = new Card
                {
                    ProductId = product.ProductId,
                    ChecklistItemId = item.ChecklistItemId,
                    Subject = item.Subject,
                    Team = item.Team,
                    Year = product.Year,
                    Set = product.DisplayName,
                    CardNumber = item.CardNumber,
                    Variety = string.Join(" - ", new[] { item.Subset, item.Parallel, item.Variation }.Where(v => !string.IsNullOrWhiteSpace(v))),
                    Serial = item.SerialNumber,
                    Category = "Baseball",
                    IsRookie = item.IsRookie,
                    IsAutograph = item.IsAutograph,
                    IsRelic = item.IsRelic,
                    IsRefractor = item.IsRefractor,
                    Quantity = row.Quantity,
                    StockImageUrl = item.StockImageUrl,
                    ImageSourcePreference = string.IsNullOrWhiteSpace(item.StockImageUrl) ? "Scan" : "Stock",
                    ListingStatus = "Draft",
                    CreatedDate = DateTime.Now
                };
                _context.Cards.Add(card);
                cards.Add(card);
            }

            if (card != null)
            {
                card.Quantity = Math.Max(0, row.Quantity);
                card.Subject = item.Subject;
                card.Team = item.Team;
                card.CardNumber = item.CardNumber;
                card.Variety = string.Join(" - ", new[] { item.Subset, item.Parallel, item.Variation }.Where(v => !string.IsNullOrWhiteSpace(v)));
                card.Serial = item.SerialNumber;
                card.IsRookie = item.IsRookie;
                card.IsAutograph = item.IsAutograph;
                card.IsRelic = item.IsRelic;
                card.IsRefractor = item.IsRefractor;
                card.StockImageUrl = item.StockImageUrl;
                card.ListingPrice = row.ListingPrice;
                card.GradeIssuer = Null(row.GradeIssuer ?? string.Empty);
                card.Grade = Null(row.Grade ?? string.Empty);
                card.CertNumber = Null(row.CertNumber ?? string.Empty);
                card.EbaySku = Null(row.EbaySku ?? string.Empty);
                card.EbayTitle = Null(row.EbayTitle ?? string.Empty);
                card.EbayDescription = Null(row.EbayDescription ?? string.Empty);
                card.EbayCategoryId = Null(row.EbayCategoryId ?? string.Empty);
                card.EbayCondition = Null(row.EbayCondition ?? string.Empty);
                card.ListingFormat = string.IsNullOrWhiteSpace(row.ListingFormat) ? "FixedPrice" : row.ListingFormat;
                card.BestOfferEnabled = row.BestOfferEnabled;
                card.ShippingPolicyName = Null(row.ShippingPolicyName ?? string.Empty);
                card.ReturnPolicyName = Null(row.ReturnPolicyName ?? string.Empty);
                card.PaymentPolicyName = Null(row.PaymentPolicyName ?? string.Empty);
                card.PackageWeightOz = row.PackageWeightOz;
                card.PackageLengthIn = row.PackageLengthIn;
                card.PackageWidthIn = row.PackageWidthIn;
                card.PackageHeightIn = row.PackageHeightIn;
                card.ListingStatus = row.Quantity > 0 ? "Draft" : "Not Listed";
                card.ModifiedDate = DateTime.Now;
            }
            updatedRows++;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Saved {updatedRows} checklist rows and synchronized inventory quantities.";
        return RedirectToAction(nameof(EditSet), new { productId = model.ProductId });
    }

    private static async Task<List<List<string>>> ReadChecklistRowsAsync(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is ".xlsx" or ".xlsm")
        {
            await using var memory = new MemoryStream();
            await file.CopyToAsync(memory);
            memory.Position = 0;
            using var workbook = new XLWorkbook(memory);
            var sheet = workbook.Worksheets.First();
            var range = sheet.RangeUsed();
            if (range == null) return new();
            return range.Rows().Select(row => row.Cells(1, range.ColumnCount()).Select(cell => cell.GetFormattedString()).ToList()).ToList();
        }
        if (extension != ".csv") throw new InvalidOperationException("Only CSV, XLSX, and XLSM checklist files are supported.");
        using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8, true);
        var rows = new List<List<string>>();
        while (!reader.EndOfStream) rows.Add(ParseCsv((await reader.ReadLineAsync()) ?? string.Empty));
        return rows;
    }

    private static string NormalizeImageKey(string value) => new string(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
    private static string? Null(string value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static string BuildChecklistSearchQuery(Product product)
    {
        var year = product.Year.ToString();
        var manufacturer = product.Brand?.BrandName ?? string.Empty;
        var brand = product.Brand?.BrandName ?? string.Empty;
        var productName = product.ProductName;
        var edition = product.ProductName;
        var sport = product.Sport?.SportName ?? string.Empty;

        return string.Join(" ", new[]
        {
            year,
            manufacturer,
            brand,
            productName,
            edition,
            sport,
            "checklist",
            "card numbers"
        }.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static ChecklistStatus ResolveChecklistStatus(
        int checklistCardCount,
        int checklistSectionCount,
        bool hasSourceProvenance,
        bool importFailed,
        bool requiresReview)
    {
        if (importFailed)
        {
            return ChecklistStatus.ChecklistImportFailed;
        }

        if (checklistCardCount <= 0)
        {
            return ChecklistStatus.ChecklistUnavailable;
        }

        if (requiresReview)
        {
            return ChecklistStatus.ChecklistRequiresReview;
        }

        if (checklistSectionCount <= 0 || !hasSourceProvenance)
        {
            return ChecklistStatus.ChecklistPartiallyLoaded;
        }

        return ChecklistStatus.ChecklistLoaded;
    }

    private static List<string> ParseCsv(string line)
    {
        var values = new List<string>(); var current = new StringBuilder(); bool quoted = false;
        for (int i = 0; i < line.Length; i++) { var c = line[i]; if (c == '"') { if (quoted && i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; } else quoted = !quoted; } else if (c == ',' && !quoted) { values.Add(current.ToString()); current.Clear(); } else current.Append(c); }
        values.Add(current.ToString()); return values;
    }
    private async Task LoadProducts(Guid? selected) => ViewBag.Products = new SelectList(await _context.Products.OrderByDescending(p => p.Year).ThenBy(p => p.DisplayName).ToListAsync(), "ProductId", "DisplayName", selected);
}

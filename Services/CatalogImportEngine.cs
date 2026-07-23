using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using IBSCardManager.Data;
using IBSCardManager.Entities;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Services;

public sealed class CatalogImportEngine : ICatalogImportService
{
    private readonly ApplicationDbContext _context;

    public CatalogImportEngine(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CatalogImportResult> ImportAsync(
        CatalogImportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.SourceName))
        {
            return Failed("SourceName is required.");
        }

        if (string.IsNullOrWhiteSpace(request.SourceLocation))
        {
            return Failed("SourceLocation is required.");
        }

        if (!File.Exists(request.SourceLocation))
        {
            return Failed($"Import file was not found: {request.SourceLocation}");
        }

        IReadOnlyList<CatalogImportRow> rows;
        try
        {
            rows = await ReadRowsAsync(request.SourceLocation, cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or JsonException)
        {
            return Failed($"Unable to read catalog import file: {ex.Message}");
        }

        if (rows.Count == 0)
        {
            return Failed("The import file does not contain any catalog rows.");
        }

        var validationErrors = Validate(rows);
        if (validationErrors.Count > 0)
        {
            return Failed("Catalog validation failed: " + string.Join("; ", validationErrors.Take(10)));
        }

        var importedProducts = 0;
        var importedCards = 0;
        var importedAt = DateTime.UtcNow;

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var sports = await _context.Sports.ToListAsync(cancellationToken);
            var brands = await _context.Brands.ToListAsync(cancellationToken);
            var products = await _context.Products.ToListAsync(cancellationToken);
            var checklistItems = await _context.ChecklistItems.ToListAsync(cancellationToken);

            foreach (var row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var sport = sports.FirstOrDefault(x => Same(x.SportName, row.Sport));
                if (sport is null)
                {
                    sport = new Sport { SportName = row.Sport.Trim(), IsActive = true };
                    sports.Add(sport);
                    _context.Sports.Add(sport);
                }

                var brand = brands.FirstOrDefault(x => Same(x.BrandName, row.Brand));
                if (brand is null)
                {
                    brand = new Brand { BrandName = row.Brand.Trim(), IsActive = true };
                    brands.Add(brand);
                    _context.Brands.Add(brand);
                }

                var product = products.FirstOrDefault(x =>
                    x.Year == row.Year &&
                    x.BrandId == brand.BrandId &&
                    Same(x.ProductName, row.Product));

                if (product is null)
                {
                    product = new Product
                    {
                        Year = row.Year,
                        ProductName = row.Product.Trim(),
                        DisplayName = string.IsNullOrWhiteSpace(row.DisplayName)
                            ? $"{row.Year} {row.Brand.Trim()} {row.Product.Trim()}"
                            : row.DisplayName.Trim(),
                        SportId = sport.SportId,
                        BrandId = brand.BrandId,
                        CatalogRecordId = StableId("product", row.Year.ToString(CultureInfo.InvariantCulture), row.Brand, row.Product),
                        CatalogSource = request.SourceName.Trim(),
                        CatalogSourceRecordId = Clean(row.SourceProductIdentifier),
                        CatalogVersion = Clean(request.SourceVersion),
                        CatalogUpdatedAt = importedAt,
                        IsVerified = true,
                        ChecklistAvailabilityStatus = "Checklist imported",
                        LastChecklistImportSource = request.SourceName.Trim(),
                        ChecklistLastImportedUtc = importedAt
                    };
                    products.Add(product);
                    _context.Products.Add(product);
                    importedProducts++;
                }
                else
                {
                    product.DisplayName = string.IsNullOrWhiteSpace(row.DisplayName)
                        ? product.DisplayName
                        : row.DisplayName.Trim();
                    product.CatalogVersion = Clean(request.SourceVersion) ?? product.CatalogVersion;
                    product.CatalogUpdatedAt = importedAt;
                    product.ChecklistAvailabilityStatus = "Checklist imported";
                    product.LastChecklistImportSource = request.SourceName.Trim();
                    product.ChecklistLastImportedUtc = importedAt;
                }

                var item = checklistItems.FirstOrDefault(x =>
                    x.ProductId == product.ProductId &&
                    Same(x.CardNumber, row.CardNumber) &&
                    Same(x.Subject, row.Subject) &&
                    Same(x.Parallel, row.Parallel) &&
                    Same(x.Variation, row.Variation));

                if (item is null)
                {
                    item = new ChecklistItem
                    {
                        ProductId = product.ProductId,
                        CardNumber = row.CardNumber.Trim(),
                        Subject = row.Subject.Trim(),
                        Team = Clean(row.Team),
                        Subset = Clean(row.Subset),
                        Parallel = Clean(row.Parallel),
                        Variation = Clean(row.Variation),
                        SerialNumber = Clean(row.SerialNumber),
                        PrintRun = Clean(row.PrintRun),
                        IsRookie = row.IsRookie,
                        IsAutograph = row.IsAutograph,
                        IsRelic = row.IsRelic,
                        IsRefractor = row.IsRefractor,
                        StockImageUrl = Clean(row.FrontImageUrl),
                        StockBackImageUrl = Clean(row.BackImageUrl),
                        SourceName = request.SourceName.Trim(),
                        SourceType = Path.GetExtension(request.SourceLocation).TrimStart('.').ToUpperInvariant(),
                        SourceFile = Path.GetFileName(request.SourceLocation),
                        SourceProductIdentifier = Clean(row.SourceProductIdentifier),
                        SourceCardIdentifier = Clean(row.SourceCardIdentifier),
                        SourceDateImportedUtc = importedAt,
                        SourceVersion = Clean(request.SourceVersion),
                        CatalogRecordId = StableId("card", row.Year.ToString(CultureInfo.InvariantCulture), row.Brand, row.Product, row.CardNumber, row.Subject, row.Parallel, row.Variation),
                        CatalogSource = request.SourceName.Trim(),
                        CatalogSourceRecordId = Clean(row.SourceCardIdentifier),
                        CatalogVersion = Clean(request.SourceVersion),
                        CatalogUpdatedAt = importedAt,
                        IsVerified = true
                    };
                    checklistItems.Add(item);
                    _context.ChecklistItems.Add(item);
                    importedCards++;
                }
                else
                {
                    item.Team = Clean(row.Team) ?? item.Team;
                    item.Subset = Clean(row.Subset) ?? item.Subset;
                    item.SerialNumber = Clean(row.SerialNumber) ?? item.SerialNumber;
                    item.PrintRun = Clean(row.PrintRun) ?? item.PrintRun;
                    item.StockImageUrl = Clean(row.FrontImageUrl) ?? item.StockImageUrl;
                    item.StockBackImageUrl = Clean(row.BackImageUrl) ?? item.StockBackImageUrl;
                    item.IsRookie |= row.IsRookie;
                    item.IsAutograph |= row.IsAutograph;
                    item.IsRelic |= row.IsRelic;
                    item.IsRefractor |= row.IsRefractor;
                    item.CatalogUpdatedAt = importedAt;
                    item.CatalogVersion = Clean(request.SourceVersion) ?? item.CatalogVersion;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Failed($"Catalog import was rolled back: {ex.Message}");
        }

        return new CatalogImportResult
        {
            Success = true,
            Message = $"Catalog import completed from {Path.GetFileName(request.SourceLocation)}.",
            ImportedProducts = importedProducts,
            ImportedChecklistCards = importedCards
        };
    }

    private static async Task<IReadOnlyList<CatalogImportRow>> ReadRowsAsync(string path, CancellationToken cancellationToken)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".csv" => await ReadCsvAsync(path, cancellationToken),
            ".json" => await ReadJsonAsync(path, cancellationToken),
            ".xlsx" => ReadExcel(path),
            _ => throw new InvalidDataException("Supported catalog formats are .csv, .json, and .xlsx.")
        };
    }

    private static async Task<IReadOnlyList<CatalogImportRow>> ReadJsonAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<List<CatalogImportRow>>(
                   stream,
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                   cancellationToken)
               ?? [];
    }

    private static async Task<IReadOnlyList<CatalogImportRow>> ReadCsvAsync(string path, CancellationToken cancellationToken)
    {
        var lines = await File.ReadAllLinesAsync(path, cancellationToken);
        if (lines.Length < 2) return [];

        var headers = ParseCsvLine(lines[0]);
        var rows = new List<CatalogImportRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            if (string.IsNullOrWhiteSpace(lines[index])) continue;
            rows.Add(MapRow(headers, ParseCsvLine(lines[index])));
        }
        return rows;
    }

    private static IReadOnlyList<CatalogImportRow> ReadExcel(string path)
    {
        using var workbook = new XLWorkbook(path);
        var worksheet = workbook.Worksheets.First();
        var usedRange = worksheet.RangeUsed();
        if (usedRange is null) return [];

        var headers = usedRange.FirstRow().Cells().Select(x => x.GetString()).ToList();
        return usedRange.RowsUsed().Skip(1)
            .Select(row => MapRow(headers, row.Cells(1, headers.Count).Select(x => x.GetFormattedString()).ToList()))
            .ToList();
    }

    private static CatalogImportRow MapRow(IReadOnlyList<string> headers, IReadOnlyList<string> values)
    {
        var map = headers.Select((header, index) => new { Key = NormalizeHeader(header), Value = index < values.Count ? values[index] : string.Empty })
            .GroupBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.First().Value, StringComparer.OrdinalIgnoreCase);

        string Get(params string[] names) => names.Select(NormalizeHeader).Where(map.ContainsKey).Select(x => map[x]).FirstOrDefault() ?? string.Empty;
        bool GetBool(params string[] names) => ParseBool(Get(names));
        int.TryParse(Get("year"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var year);

        return new CatalogImportRow
        {
            Year = year,
            Sport = Get("sport"), Brand = Get("brand", "manufacturer"), Product = Get("product", "set"), DisplayName = Get("displayname"),
            CardNumber = Get("cardnumber", "cardno", "number"), Subject = Get("subject", "player", "playername"), Team = Get("team"),
            Subset = Get("subset", "insert"), Parallel = Get("parallel"), Variation = Get("variation"), SerialNumber = Get("serialnumber", "serial"),
            PrintRun = Get("printrun"), IsRookie = GetBool("isrookie", "rookie", "rc"), IsAutograph = GetBool("isautograph", "autograph", "auto"),
            IsRelic = GetBool("isrelic", "relic", "memorabilia"), IsRefractor = GetBool("isrefractor", "refractor"),
            FrontImageUrl = Get("frontimageurl", "imageurl"), BackImageUrl = Get("backimageurl"),
            SourceProductIdentifier = Get("sourceproductidentifier", "productid"), SourceCardIdentifier = Get("sourcecardidentifier", "cardid")
        };
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var quoted = false;
        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];
            if (character == '"')
            {
                if (quoted && i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
                else quoted = !quoted;
            }
            else if (character == ',' && !quoted) { values.Add(current.ToString()); current.Clear(); }
            else current.Append(character);
        }
        values.Add(current.ToString());
        return values;
    }

    private static IReadOnlyList<string> Validate(IReadOnlyList<CatalogImportRow> rows)
    {
        var errors = new List<string>();
        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var number = index + 2;
            if (row.Year is < 1800 or > 2200) errors.Add($"row {number}: invalid year");
            if (string.IsNullOrWhiteSpace(row.Sport)) errors.Add($"row {number}: sport is required");
            if (string.IsNullOrWhiteSpace(row.Brand)) errors.Add($"row {number}: brand is required");
            if (string.IsNullOrWhiteSpace(row.Product)) errors.Add($"row {number}: product is required");
            if (string.IsNullOrWhiteSpace(row.CardNumber)) errors.Add($"row {number}: card number is required");
            if (string.IsNullOrWhiteSpace(row.Subject)) errors.Add($"row {number}: subject is required");
        }
        return errors;
    }

    private static bool Same(string? left, string? right) => string.Equals(Clean(left), Clean(right), StringComparison.OrdinalIgnoreCase);
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static bool ParseBool(string value) => value.Trim().ToLowerInvariant() is "true" or "yes" or "y" or "1" or "x";
    private static string NormalizeHeader(string value) => new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
    private static string StableId(params string?[] values)
    {
        var normalized = string.Join('|', values.Select(x => Clean(x)?.ToUpperInvariant() ?? string.Empty));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }

    private static CatalogImportResult Failed(string message) => new() { Success = false, Message = message };
}

public sealed class CatalogImportRow
{
    public int Year { get; set; }
    public string Sport { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Team { get; set; } = string.Empty;
    public string Subset { get; set; } = string.Empty;
    public string Parallel { get; set; } = string.Empty;
    public string Variation { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string PrintRun { get; set; } = string.Empty;
    public bool IsRookie { get; set; }
    public bool IsAutograph { get; set; }
    public bool IsRelic { get; set; }
    public bool IsRefractor { get; set; }
    public string FrontImageUrl { get; set; } = string.Empty;
    public string BackImageUrl { get; set; } = string.Empty;
    public string SourceProductIdentifier { get; set; } = string.Empty;
    public string SourceCardIdentifier { get; set; } = string.Empty;
}

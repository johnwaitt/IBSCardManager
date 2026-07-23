using IBSCardManager.Data;
using IBSCardManager.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Tests;

public sealed class CatalogImportEngineTests
{
    [Fact]
    public async Task ImportAsync_ImportsCsvAndSkipsDuplicatesOnSecondRun()
    {
        var file = Path.GetTempFileName();
        var csvPath = Path.ChangeExtension(file, ".csv");
        File.Move(file, csvPath);

        await File.WriteAllTextAsync(csvPath,
            "Year,Sport,Brand,Product,CardNumber,Subject,Team,IsRookie,Parallel\n" +
            "2025,Baseball,Topps,Chrome,1,Example Player,Boston Red Sox,true,Refractor\n");

        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var service = new CatalogImportEngine(context);
        var request = new CatalogImportRequest
        {
            SourceName = "Unit Test",
            SourceVersion = "1.0",
            SourceLocation = csvPath
        };

        try
        {
            var first = await service.ImportAsync(request);
            var second = await service.ImportAsync(request);

            Assert.True(first.Success, first.Message);
            Assert.Equal(1, first.ImportedProducts);
            Assert.Equal(1, first.ImportedChecklistCards);
            Assert.True(second.Success, second.Message);
            Assert.Equal(0, second.ImportedProducts);
            Assert.Equal(0, second.ImportedChecklistCards);
            Assert.Equal(1, await context.Products.CountAsync());
            Assert.Equal(1, await context.ChecklistItems.CountAsync());

            var card = await context.ChecklistItems.SingleAsync();
            Assert.Equal("Example Player", card.Subject);
            Assert.Equal("Refractor", card.Parallel);
            Assert.True(card.IsRookie);
            Assert.False(string.IsNullOrWhiteSpace(card.CatalogRecordId));
        }
        finally
        {
            File.Delete(csvPath);
        }
    }

    [Fact]
    public async Task ImportAsync_RejectsRowsMissingRequiredValues()
    {
        var file = Path.GetTempFileName();
        var csvPath = Path.ChangeExtension(file, ".csv");
        File.Move(file, csvPath);
        await File.WriteAllTextAsync(csvPath, "Year,Sport,Brand,Product,CardNumber,Subject\n2025,Baseball,Topps,Chrome,,\n");

        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        try
        {
            var result = await new CatalogImportEngine(context).ImportAsync(new CatalogImportRequest
            {
                SourceName = "Unit Test",
                SourceLocation = csvPath
            });

            Assert.False(result.Success);
            Assert.Contains("validation failed", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(context.ChecklistItems);
        }
        finally
        {
            File.Delete(csvPath);
        }
    }
}

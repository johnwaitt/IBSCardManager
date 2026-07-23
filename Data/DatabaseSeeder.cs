using IBSCardManager.Entities;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context)
        {
            await context.Database.MigrateAsync();
            await EnsureSchemaCompatibilityAsync(context);

            var baseball = await context.Sports
                .SingleAsync(sport =>
                    sport.SportName == "Baseball");

            var topps = await context.Brands
                .SingleAsync(brand =>
                    brand.BrandName == "Topps");

            var bowman = await context.Brands
                .SingleAsync(brand =>
                    brand.BrandName == "Bowman");

            string[] toppsProducts =
            {
                "Series 1",
                "Series 2",
                "Update Series",
                "Chrome",
                "Chrome Update",
                "Chrome Black",
                "Finest",
                "Tier One",
                "Inception",
                "Museum Collection",
                "Five Star",
                "Dynasty",
                "Gilded Collection"
            };

            string[] bowmanProducts =
            {
                "Bowman",
                "Bowman Chrome",
                "Bowman Draft"
            };

            for (var year = 2020; year <= 2026; year++)
            {
                foreach (var productName in toppsProducts)
                {
                    await AddProductIfMissingAsync(
                        context,
                        baseball,
                        topps,
                        year,
                        productName);
                }

                foreach (var productName in bowmanProducts)
                {
                    await AddProductIfMissingAsync(
                        context,
                        baseball,
                        bowman,
                        year,
                        productName);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureSchemaCompatibilityAsync(
            ApplicationDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync("""
                IF COL_LENGTH('Cards', 'BestOfferEnabled') IS NULL ALTER TABLE [Cards] ADD [BestOfferEnabled] bit NOT NULL CONSTRAINT [DF_Cards_BestOfferEnabled] DEFAULT(0);
                IF COL_LENGTH('Cards', 'EbayCategoryId') IS NULL ALTER TABLE [Cards] ADD [EbayCategoryId] nvarchar(80) NULL;
                IF COL_LENGTH('Cards', 'EbayCondition') IS NULL ALTER TABLE [Cards] ADD [EbayCondition] nvarchar(80) NULL;
                IF COL_LENGTH('Cards', 'EbayDescription') IS NULL ALTER TABLE [Cards] ADD [EbayDescription] nvarchar(max) NULL;
                IF COL_LENGTH('Cards', 'EbaySku') IS NULL ALTER TABLE [Cards] ADD [EbaySku] nvarchar(80) NULL;
                IF COL_LENGTH('Cards', 'EbayTitle') IS NULL ALTER TABLE [Cards] ADD [EbayTitle] nvarchar(160) NULL;
                IF COL_LENGTH('Cards', 'ListingFormat') IS NULL ALTER TABLE [Cards] ADD [ListingFormat] nvarchar(30) NOT NULL CONSTRAINT [DF_Cards_ListingFormat] DEFAULT('FixedPrice');
                IF COL_LENGTH('Cards', 'PackageHeightIn') IS NULL ALTER TABLE [Cards] ADD [PackageHeightIn] decimal(10,2) NULL;
                IF COL_LENGTH('Cards', 'PackageLengthIn') IS NULL ALTER TABLE [Cards] ADD [PackageLengthIn] decimal(10,2) NULL;
                IF COL_LENGTH('Cards', 'PackageWeightOz') IS NULL ALTER TABLE [Cards] ADD [PackageWeightOz] decimal(10,2) NULL;
                IF COL_LENGTH('Cards', 'PackageWidthIn') IS NULL ALTER TABLE [Cards] ADD [PackageWidthIn] decimal(10,2) NULL;
                IF COL_LENGTH('Cards', 'PaymentPolicyName') IS NULL ALTER TABLE [Cards] ADD [PaymentPolicyName] nvarchar(150) NULL;
                IF COL_LENGTH('Cards', 'ReturnPolicyName') IS NULL ALTER TABLE [Cards] ADD [ReturnPolicyName] nvarchar(150) NULL;
                IF COL_LENGTH('Cards', 'ShippingPolicyName') IS NULL ALTER TABLE [Cards] ADD [ShippingPolicyName] nvarchar(150) NULL;

                IF COL_LENGTH('ChecklistItems', 'Parallel') IS NULL ALTER TABLE [ChecklistItems] ADD [Parallel] nvarchar(200) NULL;
                IF COL_LENGTH('ChecklistItems', 'Position') IS NULL ALTER TABLE [ChecklistItems] ADD [Position] nvarchar(100) NULL;
                IF COL_LENGTH('ChecklistItems', 'PrintRun') IS NULL ALTER TABLE [ChecklistItems] ADD [PrintRun] nvarchar(100) NULL;
                IF COL_LENGTH('ChecklistItems', 'SerialNumber') IS NULL ALTER TABLE [ChecklistItems] ADD [SerialNumber] nvarchar(100) NULL;
                IF COL_LENGTH('ChecklistItems', 'StockBackImageUrl') IS NULL ALTER TABLE [ChecklistItems] ADD [StockBackImageUrl] nvarchar(1000) NULL;
                IF COL_LENGTH('ChecklistItems', 'Variation') IS NULL ALTER TABLE [ChecklistItems] ADD [Variation] nvarchar(200) NULL;
                IF COL_LENGTH('ChecklistItems', 'AdditionalSubjects') IS NULL ALTER TABLE [ChecklistItems] ADD [AdditionalSubjects] nvarchar(500) NULL;
                IF COL_LENGTH('ChecklistItems', 'AdditionalTeams') IS NULL ALTER TABLE [ChecklistItems] ADD [AdditionalTeams] nvarchar(500) NULL;
                IF COL_LENGTH('ChecklistItems', 'SourceName') IS NULL ALTER TABLE [ChecklistItems] ADD [SourceName] nvarchar(120) NULL;
                IF COL_LENGTH('ChecklistItems', 'SourceType') IS NULL ALTER TABLE [ChecklistItems] ADD [SourceType] nvarchar(60) NULL;
                IF COL_LENGTH('ChecklistItems', 'SourceUrl') IS NULL ALTER TABLE [ChecklistItems] ADD [SourceUrl] nvarchar(1000) NULL;
                IF COL_LENGTH('ChecklistItems', 'SourceFile') IS NULL ALTER TABLE [ChecklistItems] ADD [SourceFile] nvarchar(500) NULL;
                IF COL_LENGTH('ChecklistItems', 'SourceProductIdentifier') IS NULL ALTER TABLE [ChecklistItems] ADD [SourceProductIdentifier] nvarchar(150) NULL;
                IF COL_LENGTH('ChecklistItems', 'SourceCardIdentifier') IS NULL ALTER TABLE [ChecklistItems] ADD [SourceCardIdentifier] nvarchar(150) NULL;
                IF COL_LENGTH('ChecklistItems', 'SourceDateRetrievedUtc') IS NULL ALTER TABLE [ChecklistItems] ADD [SourceDateRetrievedUtc] datetime2 NULL;
                IF COL_LENGTH('ChecklistItems', 'SourceDateImportedUtc') IS NULL ALTER TABLE [ChecklistItems] ADD [SourceDateImportedUtc] datetime2 NULL;
                IF COL_LENGTH('ChecklistItems', 'SourceVersion') IS NULL ALTER TABLE [ChecklistItems] ADD [SourceVersion] nvarchar(80) NULL;
                IF COL_LENGTH('ChecklistItems', 'SourceLicenseUsageNotes') IS NULL ALTER TABLE [ChecklistItems] ADD [SourceLicenseUsageNotes] nvarchar(500) NULL;
                IF COL_LENGTH('ChecklistItems', 'ImportProfile') IS NULL ALTER TABLE [ChecklistItems] ADD [ImportProfile] nvarchar(120) NULL;
                IF COL_LENGTH('ChecklistItems', 'SourceOriginalRowNumber') IS NULL ALTER TABLE [ChecklistItems] ADD [SourceOriginalRowNumber] int NULL;
                IF COL_LENGTH('ChecklistItems', 'SourceRawValuesJson') IS NULL ALTER TABLE [ChecklistItems] ADD [SourceRawValuesJson] nvarchar(2000) NULL;
                IF COL_LENGTH('ChecklistItems', 'SourceVerificationStatus') IS NULL ALTER TABLE [ChecklistItems] ADD [SourceVerificationStatus] nvarchar(80) NULL;
                IF COL_LENGTH('ChecklistItems', 'ReferenceImageUrl') IS NULL ALTER TABLE [ChecklistItems] ADD [ReferenceImageUrl] nvarchar(1000) NULL;
                IF COL_LENGTH('ChecklistItems', 'ReferencePageUrl') IS NULL ALTER TABLE [ChecklistItems] ADD [ReferencePageUrl] nvarchar(1000) NULL;
                IF COL_LENGTH('ChecklistItems', 'ReferenceImageSource') IS NULL ALTER TABLE [ChecklistItems] ADD [ReferenceImageSource] nvarchar(120) NULL;
                IF COL_LENGTH('ChecklistItems', 'ReferenceImageDateLocatedUtc') IS NULL ALTER TABLE [ChecklistItems] ADD [ReferenceImageDateLocatedUtc] datetime2 NULL;
                IF COL_LENGTH('ChecklistItems', 'ReferenceImageUsageStatus') IS NULL ALTER TABLE [ChecklistItems] ADD [ReferenceImageUsageStatus] nvarchar(80) NULL;
                IF COL_LENGTH('ChecklistItems', 'CachedThumbnailPath') IS NULL ALTER TABLE [ChecklistItems] ADD [CachedThumbnailPath] nvarchar(500) NULL;
                IF COL_LENGTH('ChecklistItems', 'ReferenceImageHash') IS NULL ALTER TABLE [ChecklistItems] ADD [ReferenceImageHash] nvarchar(200) NULL;
                IF COL_LENGTH('ChecklistItems', 'ReferenceImageVerificationStatus') IS NULL ALTER TABLE [ChecklistItems] ADD [ReferenceImageVerificationStatus] nvarchar(80) NULL;

                IF COL_LENGTH('Products', 'ChecklistAvailabilityStatus') IS NULL ALTER TABLE [Products] ADD [ChecklistAvailabilityStatus] nvarchar(80) NOT NULL CONSTRAINT [DF_Products_ChecklistAvailabilityStatus] DEFAULT('Checklist unavailable');
                IF COL_LENGTH('Products', 'LastChecklistImportSource') IS NULL ALTER TABLE [Products] ADD [LastChecklistImportSource] nvarchar(200) NULL;
                IF COL_LENGTH('Products', 'ChecklistLastImportedUtc') IS NULL ALTER TABLE [Products] ADD [ChecklistLastImportedUtc] datetime2 NULL;

                IF OBJECT_ID('ChecklistImportHistories', 'U') IS NULL
                BEGIN
                    CREATE TABLE [ChecklistImportHistories](
                        [ChecklistImportHistoryId] uniqueidentifier NOT NULL,
                        [ProductId] uniqueidentifier NOT NULL,
                        [SourceName] nvarchar(120) NOT NULL,
                        [SourceType] nvarchar(60) NOT NULL,
                        [SourceUrl] nvarchar(1000) NULL,
                        [SourceFile] nvarchar(500) NULL,
                        [SourceProductIdentifier] nvarchar(150) NULL,
                        [SourceVersion] nvarchar(80) NULL,
                        [LicenseUsageNotes] nvarchar(500) NULL,
                        [ImportProfile] nvarchar(120) NULL,
                        [VerificationStatus] nvarchar(80) NOT NULL,
                        [RetrievedUtc] datetime2 NULL,
                        [ImportedUtc] datetime2 NOT NULL,
                        [RowsRead] int NOT NULL,
                        [RowsImported] int NOT NULL,
                        [RowsUpdated] int NOT NULL,
                        [Notes] nvarchar(1000) NULL,
                        CONSTRAINT [PK_ChecklistImportHistories] PRIMARY KEY ([ChecklistImportHistoryId]),
                        CONSTRAINT [FK_ChecklistImportHistories_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products]([ProductId]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_ChecklistImportHistories_ProductId_ImportedUtc] ON [ChecklistImportHistories]([ProductId], [ImportedUtc]);
                END;

                IF OBJECT_ID('WebSearchResults', 'U') IS NULL
                BEGIN
                    CREATE TABLE [WebSearchResults](
                        [WebSearchResultId] uniqueidentifier NOT NULL,
                        [ProductId] uniqueidentifier NULL,
                        [SearchScope] nvarchar(40) NOT NULL,
                        [SearchQuery] nvarchar(1000) NOT NULL,
                        [Title] nvarchar(300) NULL,
                        [PageSource] nvarchar(200) NULL,
                        [PageUrl] nvarchar(1000) NULL,
                        [ImageUrl] nvarchar(1000) NULL,
                        [DateRetrievedUtc] datetime2 NOT NULL,
                        [UserConfirmed] bit NOT NULL,
                        [MetadataJson] nvarchar(2000) NULL,
                        CONSTRAINT [PK_WebSearchResults] PRIMARY KEY ([WebSearchResultId]),
                        CONSTRAINT [FK_WebSearchResults_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products]([ProductId]) ON DELETE SET NULL
                    );
                    CREATE INDEX [IX_WebSearchResults_ProductId_SearchScope_DateRetrievedUtc] ON [WebSearchResults]([ProductId], [SearchScope], [DateRetrievedUtc]);
                END;
                """);
        }

        private static async Task AddProductIfMissingAsync(
            ApplicationDbContext context,
            Sport sport,
            Brand brand,
            int year,
            string productName)
        {
            var exists = await context.Products.AnyAsync(product =>
                product.Year == year &&
                product.BrandId == brand.BrandId &&
                product.ProductName == productName);

            if (exists)
            {
                return;
            }

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                Year = year,
                ProductName = productName,
                DisplayName =
                    $"{year} {brand.BrandName} {productName}",
                SportId = sport.SportId,
                BrandId = brand.BrandId,
                IsActive = true
            });
        }
    }
}
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
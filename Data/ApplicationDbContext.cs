using IBSCardManager.Entities;
using Microsoft.EntityFrameworkCore;

namespace IBSCardManager.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Card> Cards { get; set; } = null!;
        public DbSet<Sport> Sports { get; set; } = null!;
        public DbSet<Brand> Brands { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var baseballId =
                Guid.Parse("11111111-1111-1111-1111-111111111111");

            var toppsId =
                Guid.Parse("22222222-2222-2222-2222-222222222222");

            var bowmanId =
                Guid.Parse("33333333-3333-3333-3333-333333333333");

            modelBuilder.Entity<Card>()
                .HasIndex(card => card.CertNumber);

            modelBuilder.Entity<Card>()
                .HasIndex(card => card.Subject);

            modelBuilder.Entity<Card>()
                .HasIndex(card => card.Team);

            modelBuilder.Entity<Card>()
                .HasIndex(card => card.Set);

            modelBuilder.Entity<Card>()
                .HasIndex(card => card.CardNumber);

            modelBuilder.Entity<Card>()
                .HasOne(card => card.Product)
                .WithMany()
                .HasForeignKey(card => card.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Product>()
                .HasOne(product => product.Sport)
                .WithMany(sport => sport.Products)
                .HasForeignKey(product => product.SportId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(product => product.Brand)
                .WithMany(brand => brand.Products)
                .HasForeignKey(product => product.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasIndex(product => new
                {
                    product.Year,
                    product.BrandId,
                    product.ProductName
                })
                .IsUnique();

            modelBuilder.Entity<Sport>().HasData(
                new Sport
                {
                    SportId = baseballId,
                    SportName = "Baseball",
                    IsActive = true
                });

            modelBuilder.Entity<Brand>().HasData(
                new Brand
                {
                    BrandId = toppsId,
                    BrandName = "Topps",
                    IsActive = true
                },
                new Brand
                {
                    BrandId = bowmanId,
                    BrandName = "Bowman",
                    IsActive = true
                });
        }
    }
}
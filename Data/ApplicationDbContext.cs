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
        public DbSet<ChecklistItem> ChecklistItems { get; set; } = null!;
        public DbSet<ChecklistImportHistory> ChecklistImportHistories { get; set; } = null!;
        public DbSet<WebSearchResult> WebSearchResults { get; set; } = null!;
        public DbSet<CollectionSnapshot> CollectionSnapshots { get; set; } = null!;
        public DbSet<InventoryAnalyticsSummary> InventoryAnalyticsSummaries { get; set; } = null!;
        public DbSet<RecommendationRecord> RecommendationRecords { get; set; } = null!;
        public DbSet<UserAnalyticsPreference> UserAnalyticsPreferences { get; set; } = null!;
        public DbSet<KnowledgeRecord> KnowledgeRecords { get; set; } = null!;
        public DbSet<KnowledgeEvidence> KnowledgeEvidence { get; set; } = null!;
        public DbSet<UserCorrection> UserCorrections { get; set; } = null!;
        public DbSet<DecisionHistoryRecord> DecisionHistoryRecords { get; set; } = null!;
        public DbSet<KnowledgeReviewItem> KnowledgeReviewItems { get; set; } = null!;
        public DbSet<KnowledgeAuditRecord> KnowledgeAuditRecords { get; set; } = null!;

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
                .HasIndex(card => card.CatalogRecordId);

            modelBuilder.Entity<Card>()
                .HasIndex(card => new { card.ProductId, card.ChecklistItemId, card.CatalogSourceRecordId });

            modelBuilder.Entity<Card>()
                .HasIndex(card => new { card.CatalogVersion, card.IsVerified, card.IsDeprecated });

            modelBuilder.Entity<Card>()
                .HasOne(card => card.Product)
                .WithMany()
                .HasForeignKey(card => card.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Card>()
                .HasOne(card => card.ChecklistItem)
                .WithMany()
                .HasForeignKey(card => card.ChecklistItemId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ChecklistItem>()
                .HasOne(item => item.Product)
                .WithMany(product => product.ChecklistItems)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChecklistItem>()
                .HasIndex(item => new { item.ProductId, item.CardNumber, item.Subject });

            modelBuilder.Entity<ChecklistItem>()
                .HasIndex(item => item.CatalogRecordId);

            modelBuilder.Entity<ChecklistItem>()
                .HasIndex(item => new { item.ProductId, item.CardNumber, item.Parallel, item.Variation });

            modelBuilder.Entity<ChecklistItem>()
                .HasIndex(item => new { item.CatalogVersion, item.IsVerified, item.IsDeprecated });

            modelBuilder.Entity<ChecklistImportHistory>()
                .HasOne(history => history.Product)
                .WithMany()
                .HasForeignKey(history => history.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChecklistImportHistory>()
                .HasIndex(history => new { history.ProductId, history.ImportedUtc });

            modelBuilder.Entity<WebSearchResult>()
                .HasOne(result => result.Product)
                .WithMany()
                .HasForeignKey(result => result.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WebSearchResult>()
                .HasIndex(result => new { result.ProductId, result.SearchScope, result.DateRetrievedUtc });

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

            modelBuilder.Entity<Product>()
                .HasIndex(product => product.CatalogRecordId);

            modelBuilder.Entity<Product>()
                .HasIndex(product => new { product.CatalogSourceRecordId, product.CatalogVersion, product.IsVerified });

            modelBuilder.Entity<CollectionSnapshot>()
                .HasIndex(snapshot => snapshot.SnapshotDate);

            modelBuilder.Entity<InventoryAnalyticsSummary>()
                .HasOne(summary => summary.InventoryCard)
                .WithMany()
                .HasForeignKey(summary => summary.InventoryCardId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InventoryAnalyticsSummary>()
                .HasIndex(summary => summary.LastCalculatedAt);

            modelBuilder.Entity<RecommendationRecord>()
                .HasOne(record => record.InventoryCard)
                .WithMany()
                .HasForeignKey(record => record.InventoryCardId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecommendationRecord>()
                .HasIndex(record => new { record.InventoryCardId, record.RecommendationType, record.GeneratedAt });

            modelBuilder.Entity<UserAnalyticsPreference>()
                .HasIndex(preference => preference.ProfileName)
                .IsUnique();

            modelBuilder.Entity<KnowledgeRecord>()
                .HasIndex(record => record.StableId)
                .IsUnique();

            modelBuilder.Entity<KnowledgeRecord>()
                .HasIndex(record => new { record.SubjectType, record.SubjectStableId });

            modelBuilder.Entity<KnowledgeRecord>()
                .HasIndex(record => record.KnowledgeType);

            modelBuilder.Entity<KnowledgeRecord>()
                .HasIndex(record => record.VerificationLevel);

            modelBuilder.Entity<KnowledgeRecord>()
                .HasIndex(record => record.ConfidenceScore);

            modelBuilder.Entity<KnowledgeRecord>()
                .HasIndex(record => record.CreatedAt);

            modelBuilder.Entity<KnowledgeRecord>()
                .HasIndex(record => record.UpdatedAt);

            modelBuilder.Entity<KnowledgeEvidence>()
                .HasOne(evidence => evidence.KnowledgeRecord)
                .WithMany(record => record.Evidence)
                .HasForeignKey(evidence => evidence.KnowledgeRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<KnowledgeEvidence>()
                .HasIndex(evidence => evidence.KnowledgeRecordId);

            modelBuilder.Entity<KnowledgeReviewItem>()
                .HasIndex(item => item.Status);

            modelBuilder.Entity<KnowledgeReviewItem>()
                .HasIndex(item => item.CreatedAt);

            modelBuilder.Entity<UserCorrection>()
                .HasIndex(correction => new { correction.SubjectType, correction.SubjectStableId });

            modelBuilder.Entity<UserCorrection>()
                .HasIndex(correction => correction.LearningStatus);

            modelBuilder.Entity<DecisionHistoryRecord>()
                .HasIndex(record => new { record.SubjectType, record.SubjectStableId, record.CreatedAt });

            modelBuilder.Entity<KnowledgeAuditRecord>()
                .HasIndex(record => record.CreatedAt);

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
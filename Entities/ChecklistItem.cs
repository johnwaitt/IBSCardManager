using System.ComponentModel.DataAnnotations;

namespace IBSCardManager.Entities
{
    public class ChecklistItem
    {
        public Guid ChecklistItemId { get; set; } = Guid.NewGuid();

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        [Required, StringLength(100)]
        public string CardNumber { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [StringLength(500)]
        public string? AdditionalSubjects { get; set; }

        [StringLength(150)]
        public string? Team { get; set; }

        [StringLength(500)]
        public string? AdditionalTeams { get; set; }

        [StringLength(200)]
        public string? Subset { get; set; }

        public bool IsRookie { get; set; }
        public bool IsAutograph { get; set; }
        public bool IsRelic { get; set; }
        public bool IsRefractor { get; set; }

        [StringLength(1000)]
        public string? StockImageUrl { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }

        [StringLength(200)]
        public string? Parallel { get; set; }

        [StringLength(200)]
        public string? Variation { get; set; }

        [StringLength(100)]
        public string? SerialNumber { get; set; }

        [StringLength(100)]
        public string? PrintRun { get; set; }

        [StringLength(1000)]
        public string? StockBackImageUrl { get; set; }

        [StringLength(120)]
        public string? SourceName { get; set; }

        [StringLength(60)]
        public string? SourceType { get; set; }

        [StringLength(1000)]
        public string? SourceUrl { get; set; }

        [StringLength(500)]
        public string? SourceFile { get; set; }

        [StringLength(150)]
        public string? SourceProductIdentifier { get; set; }

        [StringLength(150)]
        public string? SourceCardIdentifier { get; set; }

        public DateTime? SourceDateRetrievedUtc { get; set; }

        public DateTime? SourceDateImportedUtc { get; set; }

        [StringLength(80)]
        public string? SourceVersion { get; set; }

        [StringLength(500)]
        public string? SourceLicenseUsageNotes { get; set; }

        [StringLength(120)]
        public string? ImportProfile { get; set; }

        public int? SourceOriginalRowNumber { get; set; }

        [StringLength(2000)]
        public string? SourceRawValuesJson { get; set; }

        [StringLength(80)]
        public string? SourceVerificationStatus { get; set; }

        [StringLength(1000)]
        public string? ReferenceImageUrl { get; set; }

        [StringLength(1000)]
        public string? ReferencePageUrl { get; set; }

        [StringLength(120)]
        public string? ReferenceImageSource { get; set; }

        public DateTime? ReferenceImageDateLocatedUtc { get; set; }

        [StringLength(80)]
        public string? ReferenceImageUsageStatus { get; set; }

        [StringLength(500)]
        public string? CachedThumbnailPath { get; set; }

        [StringLength(200)]
        public string? ReferenceImageHash { get; set; }

        [StringLength(80)]
        public string? ReferenceImageVerificationStatus { get; set; }

        [StringLength(150)]
        public string? CatalogRecordId { get; set; }

        [StringLength(120)]
        public string? CatalogSource { get; set; }

        [StringLength(150)]
        public string? CatalogSourceRecordId { get; set; }

        [StringLength(80)]
        public string? CatalogVersion { get; set; }

        public DateTime? CatalogUpdatedAt { get; set; }

        public decimal? CatalogConfidence { get; set; }

        public bool IsUserCreated { get; set; }

        public bool IsVerified { get; set; }

        public bool IsDeprecated { get; set; }

        public Guid? ReplacedByCatalogRecordId { get; set; }
    }
}

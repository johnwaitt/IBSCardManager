using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBSCardManager.Entities
{
    public class Card
    {
        public Guid CardId { get; set; } = Guid.NewGuid();

        [StringLength(20)]
        public string? AutographGrade { get; set; }

        [StringLength(500)]
        public string? BackImagePath { get; set; }

        [StringLength(100)]
        public string? CardNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = "Baseball";

        [StringLength(100)]
        public string? CertNumber { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? FrontImagePath { get; set; }

        [StringLength(1000)]
        public string? StockImageUrl { get; set; }

        [StringLength(20)]
        public string ImageSourcePreference { get; set; } = "Scan";

        [StringLength(20)]
        public string? Grade { get; set; }

        [StringLength(50)]
        public string? GradeIssuer { get; set; }

        public bool IsAutograph { get; set; }

        public bool IsRelic { get; set; }

        public bool IsRefractor { get; set; }

        public bool IsRookie { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ListingPrice { get; set; }

        [StringLength(100)]
        public string? ListingStatus { get; set; }

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MyCost { get; set; }

        public string? MyNotes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MyValue { get; set; }

        public Guid? ProductId { get; set; }

        public Product? Product { get; set; }

        public Guid? ChecklistItemId { get; set; }

        public ChecklistItem? ChecklistItem { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PsaEstimate { get; set; }

        public int Quantity { get; set; } = 1;

        [StringLength(50)]
        public string? Serial { get; set; }

        [NotMapped]
        public string? SerialNumber { get; set; }

        [NotMapped]
        public string? PrintRun { get; set; }

        [StringLength(250)]
        public string? Set { get; set; }

        [StringLength(100)]
        public string? StorageBin { get; set; }

        [StringLength(100)]
        public string? StorageBox { get; set; }

        [StringLength(100)]
        public string? StorageRow { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Player Name")]
        public string Subject { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Team { get; set; }

        [StringLength(200)]
        public string? Variety { get; set; }

        public int? Year { get; set; }

        [StringLength(80)]
        public string? EbaySku { get; set; }

        [StringLength(80)]
        public string? EbayCategoryId { get; set; }

        [StringLength(80)]
        public string? EbayCondition { get; set; }

        [StringLength(160)]
        public string? EbayTitle { get; set; }

        public string? EbayDescription { get; set; }

        [StringLength(30)]
        public string ListingFormat { get; set; } = "FixedPrice";

        public bool BestOfferEnabled { get; set; }

        [StringLength(150)]
        public string? ShippingPolicyName { get; set; }

        [StringLength(150)]
        public string? ReturnPolicyName { get; set; }

        [StringLength(150)]
        public string? PaymentPolicyName { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PackageWeightOz { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PackageLengthIn { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PackageWidthIn { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PackageHeightIn { get; set; }

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

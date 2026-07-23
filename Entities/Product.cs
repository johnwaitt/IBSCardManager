using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IBSCardManager.Entities
{
    public class Product
    {
        public Guid ProductId { get; set; } = Guid.NewGuid();

        [Required]
        public int Year { get; set; }

        [Required]
        [MaxLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string DisplayName { get; set; } = string.Empty;

        public DateTime? ReleaseDate { get; set; }

        public bool IsActive { get; set; } = true;

        public Guid SportId { get; set; }

        public Sport? Sport { get; set; }

        public Guid BrandId { get; set; }

        public Brand? Brand { get; set; }

        [MaxLength(80)]
        public string ChecklistAvailabilityStatus { get; set; } = "Checklist unavailable";

        [MaxLength(200)]
        public string? LastChecklistImportSource { get; set; }

        public DateTime? ChecklistLastImportedUtc { get; set; }

        [MaxLength(150)]
        public string? CatalogRecordId { get; set; }

        [MaxLength(120)]
        public string? CatalogSource { get; set; }

        [MaxLength(150)]
        public string? CatalogSourceRecordId { get; set; }

        [MaxLength(80)]
        public string? CatalogVersion { get; set; }

        public DateTime? CatalogUpdatedAt { get; set; }

        public decimal? CatalogConfidence { get; set; }

        public bool IsUserCreated { get; set; }

        public bool IsVerified { get; set; }

        public bool IsDeprecated { get; set; }

        public Guid? ReplacedByCatalogRecordId { get; set; }

        public ICollection<ChecklistItem> ChecklistItems { get; set; }
            = new List<ChecklistItem>();
    }
}

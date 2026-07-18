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

        [StringLength(20)]
        public string? Grade { get; set; }

        [StringLength(50)]
        public string? GradeIssuer { get; set; }

        public bool IsAutograph { get; set; }

        public bool IsRelic { get; set; }

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

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PsaEstimate { get; set; }

        public int Quantity { get; set; } = 1;

        [StringLength(50)]
        public string? Serial { get; set; }

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
        public string Subject { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Team { get; set; }

        [StringLength(200)]
        public string? Variety { get; set; }

        public int? Year { get; set; }
    }
}
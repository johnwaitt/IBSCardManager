using System.ComponentModel.DataAnnotations;

namespace IBSCardManager.Entities;

public class ChecklistImportHistory
{
    public Guid ChecklistImportHistoryId { get; set; } = Guid.NewGuid();

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    [Required, StringLength(120)]
    public string SourceName { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string SourceType { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? SourceUrl { get; set; }

    [StringLength(500)]
    public string? SourceFile { get; set; }

    [StringLength(150)]
    public string? SourceProductIdentifier { get; set; }

    [StringLength(80)]
    public string? SourceVersion { get; set; }

    [StringLength(500)]
    public string? LicenseUsageNotes { get; set; }

    [StringLength(120)]
    public string? ImportProfile { get; set; }

    [StringLength(80)]
    public string VerificationStatus { get; set; } = "Unverified";

    public DateTime? RetrievedUtc { get; set; }

    public DateTime ImportedUtc { get; set; } = DateTime.UtcNow;

    public int RowsRead { get; set; }

    public int RowsImported { get; set; }

    public int RowsUpdated { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

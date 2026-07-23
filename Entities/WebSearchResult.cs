using System.ComponentModel.DataAnnotations;

namespace IBSCardManager.Entities;

public class WebSearchResult
{
    public Guid WebSearchResultId { get; set; } = Guid.NewGuid();

    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }

    [Required, StringLength(40)]
    public string SearchScope { get; set; } = "Checklist";

    [Required, StringLength(1000)]
    public string SearchQuery { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Title { get; set; }

    [StringLength(200)]
    public string? PageSource { get; set; }

    [StringLength(1000)]
    public string? PageUrl { get; set; }

    [StringLength(1000)]
    public string? ImageUrl { get; set; }

    public DateTime DateRetrievedUtc { get; set; } = DateTime.UtcNow;

    public bool UserConfirmed { get; set; }

    [StringLength(2000)]
    public string? MetadataJson { get; set; }
}

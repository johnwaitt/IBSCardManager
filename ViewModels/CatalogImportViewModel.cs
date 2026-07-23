using Microsoft.AspNetCore.Http;
using IBSCardManager.Services;

namespace IBSCardManager.ViewModels;

public sealed class CatalogImportViewModel
{
    public IFormFile? CatalogFile { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public string? SourceVersion { get; set; }
    public CatalogImportResult? Result { get; set; }
    public string? ErrorMessage { get; set; }
}

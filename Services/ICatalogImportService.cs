namespace IBSCardManager.Services;

public interface ICatalogImportService
{
    Task<CatalogImportResult> ImportAsync(CatalogImportRequest request, CancellationToken cancellationToken = default);
}

public sealed class CatalogImportRequest
{
    public string SourceName { get; init; } = string.Empty;
    public string? SourceVersion { get; init; }
    public string? SourceLocation { get; init; }
}

public sealed class CatalogImportResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int ImportedProducts { get; init; }
    public int ImportedChecklistCards { get; init; }
}

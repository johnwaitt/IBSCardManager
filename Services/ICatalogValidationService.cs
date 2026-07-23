namespace IBSCardManager.Services;

public interface ICatalogValidationService
{
    Task<IReadOnlyList<CatalogIntegrityIssue>> RunReadinessChecksAsync(CancellationToken cancellationToken = default);
}

public sealed class CatalogIntegrityIssue
{
    public string Code { get; init; } = string.Empty;
    public string Severity { get; init; } = "Warning";
    public string Summary { get; init; } = string.Empty;
    public string Details { get; init; } = string.Empty;
}

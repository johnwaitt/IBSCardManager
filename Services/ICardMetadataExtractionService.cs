using IBSCardManager.Models;

namespace IBSCardManager.Services;

public interface ICardMetadataExtractionService
{
    Task<ScannerIdentificationResult> ExtractAsync(ScannerIdentificationRequest request, CancellationToken cancellationToken = default);
}

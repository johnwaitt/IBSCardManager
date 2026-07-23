using IBSCardManager.Models;

namespace IBSCardManager.Services;

public interface ICardImageIdentificationService
{
    Task<ScannerIdentificationResult> AnalyzePairAsync(ScannerIdentificationRequest request, CancellationToken cancellationToken = default);
}

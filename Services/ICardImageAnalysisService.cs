using IBSCardManager.Models;

namespace IBSCardManager.Services;

public interface ICardImageAnalysisService
{
    Task<ScannerIdentificationResult> AnalyzeAsync(ScannerIdentificationRequest request, CancellationToken cancellationToken = default);
}

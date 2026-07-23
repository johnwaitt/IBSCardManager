using IBSCardManager.Models;

namespace IBSCardManager.Services;

public interface ICardCandidateMatchingService
{
    Task<IReadOnlyList<ScannerCandidateResult>> FindCandidatesAsync(ScannerIdentificationResult extraction, Guid? productId = null, CancellationToken cancellationToken = default);
}

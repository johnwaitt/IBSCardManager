using IBSCardManager.Models;

namespace IBSCardManager.Services;

public interface IChecklistCandidateService
{
    Task<IReadOnlyList<ChecklistImportPreviewRowViewModel>> BuildPreviewAsync(ChecklistImportPreviewRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScannerCandidateResult>> FindLocalCandidatesAsync(ScannerStructuredSearchRequest request, CancellationToken cancellationToken = default);
}

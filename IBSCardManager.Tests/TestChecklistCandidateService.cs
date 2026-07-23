using IBSCardManager.Models;
using IBSCardManager.Services;

namespace IBSCardManager.Tests;

internal sealed class TestChecklistCandidateService : IChecklistCandidateService
{
    public Task<IReadOnlyList<ChecklistImportPreviewRowViewModel>> BuildPreviewAsync(ChecklistImportPreviewRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ChecklistImportPreviewRowViewModel>>(Array.Empty<ChecklistImportPreviewRowViewModel>());
    }

    public Task<IReadOnlyList<ScannerCandidateResult>> FindLocalCandidatesAsync(ScannerStructuredSearchRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ScannerCandidateResult>>(Array.Empty<ScannerCandidateResult>());
    }
}

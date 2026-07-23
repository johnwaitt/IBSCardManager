using IBSCardManager.Models;

namespace IBSCardManager.Services;

public sealed class NoOpCardWebSearchService : ICardWebSearchService
{
    public Task<IReadOnlyList<WebSearchCandidateViewModel>> SearchCardAsync(WebCardSearchRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<WebSearchCandidateViewModel>>(Array.Empty<WebSearchCandidateViewModel>());
    }

    public Task<IReadOnlyList<WebSearchCandidateViewModel>> SearchChecklistAsync(WebChecklistSearchRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<WebSearchCandidateViewModel>>(Array.Empty<WebSearchCandidateViewModel>());
    }
}

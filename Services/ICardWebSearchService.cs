using IBSCardManager.Models;

namespace IBSCardManager.Services;

public interface ICardWebSearchService
{
    Task<IReadOnlyList<WebSearchCandidateViewModel>> SearchCardAsync(WebCardSearchRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WebSearchCandidateViewModel>> SearchChecklistAsync(WebChecklistSearchRequest request, CancellationToken cancellationToken = default);
}

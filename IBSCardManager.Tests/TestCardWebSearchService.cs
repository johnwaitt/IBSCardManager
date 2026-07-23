using IBSCardManager.Models;
using IBSCardManager.Services;

namespace IBSCardManager.Tests;

internal sealed class TestCardWebSearchService : ICardWebSearchService
{
    public Task<IReadOnlyList<WebSearchCandidateViewModel>> SearchCardAsync(WebCardSearchRequest request, CancellationToken cancellationToken = default)
    {
        var result = new WebSearchCandidateViewModel
        {
            Title = "Candidate result",
            PageSource = "Test source",
            PageUrl = "https://example.test/card",
            ImageUrl = "https://example.test/card.jpg",
            SearchQuery = string.Join(" ", new[] { request.Player, request.Year?.ToString(), request.Product, request.CardNumber }.Where(x => !string.IsNullOrWhiteSpace(x)))
        };

        return Task.FromResult<IReadOnlyList<WebSearchCandidateViewModel>>(new[] { result });
    }

    public Task<IReadOnlyList<WebSearchCandidateViewModel>> SearchChecklistAsync(WebChecklistSearchRequest request, CancellationToken cancellationToken = default)
    {
        var result = new WebSearchCandidateViewModel
        {
            Title = "Checklist result",
            PageSource = "Test source",
            PageUrl = "https://example.test/checklist",
            SearchQuery = string.Join(" ", new[] { request.Year?.ToString(), request.Brand, request.Product, "checklist" }.Where(x => !string.IsNullOrWhiteSpace(x)))
        };

        return Task.FromResult<IReadOnlyList<WebSearchCandidateViewModel>>(new[] { result });
    }
}

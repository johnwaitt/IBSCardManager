using IBSCardManager.Models;

namespace IBSCardManager.Services;

public interface IOpenAiCardAnalysisService
{
    bool IsConfigured { get; }
    Task<CardAnalysisResponseEnvelope> AnalyzeAsync(CardAnalysisRequest request, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}
using IBSCardManager.Entities;
using IBSCardManager.Models;
using IBSCardManager.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IBSCardManager.Tests;

public sealed class KnowledgeCoreTests
{
    [Fact]
    public void ConfidenceScoring_Returns_CappedHigh_WithWeakSingleSourceGuard()
    {
        var service = new ConfidenceScoringService();
        var result = service.Calculate(new ConfidenceScoringInput
        {
            MasterCatalogExactMatch = false,
            StableIdMatch = false,
            CardNumberMatch = true,
            PlayerMatch = true,
            OcrQuality = 40,
            ImageQuality = 40,
            UserConfirmations = 0,
            MarketplaceSupportCount = 0,
            ContradictingEvidenceCount = 0,
            MissingRequiredFieldCount = 1,
            SourceFreshnessDays = 0
        });

        Assert.True(result.Score <= 69m);
        Assert.NotEqual(ConfidenceClassification.Verified, result.Classification);
    }

    [Fact]
    public void ConfidenceScoring_Drops_With_Contradictions()
    {
        var service = new ConfidenceScoringService();
        var result = service.Calculate(new ConfidenceScoringInput
        {
            MasterCatalogExactMatch = true,
            StableIdMatch = true,
            CardNumberMatch = true,
            PlayerMatch = true,
            TeamMatch = true,
            ProductMatch = true,
            YearMatch = true,
            OcrQuality = 90,
            ImageQuality = 90,
            UserConfirmations = 3,
            MarketplaceSupportCount = 2,
            ContradictingEvidenceCount = 2,
            MissingRequiredFieldCount = 0,
            SourceFreshnessDays = 0
        });

        Assert.Contains(result.ContradictingFactors, x => x.Name.Contains("Contradicting evidence"));
        Assert.True(result.Score <= 100m);
        Assert.NotEqual(ConfidenceClassification.Verified, result.Classification);
    }

    [Fact]
    public void LearningService_Blocks_HighImpact_Actions()
    {
        var service = new KnowledgeLearningService();
        var decision = service.EvaluateAutoLearningAction(CorrectionType.Identity, repeatedCorrectionCount: 5, highImpactAction: true);

        Assert.False(decision.ApplyAutomatically);
        Assert.True(decision.QueueForReview);
        Assert.Equal(KnowledgeReviewItemType.HighImpactLearningAction, decision.ReviewItemType);
    }

    [Fact]
    public void LearningService_Allows_Repeated_LowImpact_Local_Adjustment()
    {
        var service = new KnowledgeLearningService();
        var decision = service.EvaluateAutoLearningAction(CorrectionType.Ocr, repeatedCorrectionCount: 3, highImpactAction: false);

        Assert.True(decision.ApplyAutomatically);
        Assert.False(decision.QueueForReview);
    }

    [Fact]
    public void DecisionHistory_Builds_Concise_Explanation_Without_ChainOfThought()
    {
        var service = new DecisionHistoryService(new IBSCardManager.Data.ApplicationDbContext(new DbContextOptionsBuilder<IBSCardManager.Data.ApplicationDbContext>().UseSqlite("Data Source=:memory:").Options));
        var summary = service.BuildExplanationSummary(new DecisionExplanationInput
        {
            SelectedOption = "chk-125",
            ConfidenceScore = 88.5m,
            StrongestSupportingFactors = new[] { "Card number match", "Player match" },
            ImportantContradictions = new[] { "Parallel mismatch" },
            MissingInformation = new[] { "Serial maximum" },
            AlternativesConsidered = new[] { "chk-126" },
            UserAction = "Confirmed"
        });

        Assert.Contains("Selected:", summary);
        Assert.Contains("Confidence:", summary);
        Assert.DoesNotContain("reasoning", summary, StringComparison.OrdinalIgnoreCase);
    }
}

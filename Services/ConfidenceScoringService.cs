using IBSCardManager.Entities;
using IBSCardManager.Models;

namespace IBSCardManager.Services;

public sealed class ConfidenceScoringService : IConfidenceScoringService
{
    public ConfidenceScoreResult Calculate(ConfidenceScoringInput input)
    {
        var supporting = new List<ConfidenceFactor>();
        var contradicting = new List<ConfidenceFactor>();
        var missing = new List<ConfidenceFactor>();

        decimal score = 10m;

        AddSupport(input.MasterCatalogExactMatch, 25m, "Master catalog exact match", "Catalog exact match strengthens confidence.");
        AddSupport(input.StableIdMatch, 20m, "Stable ID match", "Stable identifiers align across records.");
        AddSupport(input.ExternalSourceMatch, 8m, "External source match", "External source supports the statement.");
        AddSupport(input.CardNumberMatch, 10m, "Card number match", "Card number matched expected value.");
        AddSupport(input.PlayerMatch, 8m, "Player match", "Player name matched expected value.");
        AddSupport(input.TeamMatch, 5m, "Team match", "Team matched expected value.");
        AddSupport(input.ProductMatch, 10m, "Product match", "Product matched expected value.");
        AddSupport(input.YearMatch, 7m, "Year match", "Year matched expected value.");
        AddSupport(input.ParallelMatch, 6m, "Parallel match", "Parallel matched expected value.");
        AddSupport(input.VariationMatch, 6m, "Variation match", "Variation matched expected value.");
        AddSupport(input.ReferenceImageMatch, 7m, "Reference image match", "Reference image attributes aligned.");

        var ocrImpact = decimal.Clamp(input.OcrQuality, 0m, 100m) * 0.06m;
        if (ocrImpact > 0)
        {
            score += ocrImpact;
            supporting.Add(new ConfidenceFactor { Name = "OCR quality", Impact = ocrImpact, Detail = $"OCR quality contributed {Math.Round(ocrImpact, 2)}." });
        }
        else
        {
            missing.Add(new ConfidenceFactor { Name = "OCR quality", Impact = -4m, Detail = "OCR quality is missing or zero." });
            score -= 4m;
        }

        var imageImpact = decimal.Clamp(input.ImageQuality, 0m, 100m) * 0.05m;
        if (imageImpact > 0)
        {
            score += imageImpact;
            supporting.Add(new ConfidenceFactor { Name = "Image quality", Impact = imageImpact, Detail = $"Image quality contributed {Math.Round(imageImpact, 2)}." });
        }
        else
        {
            missing.Add(new ConfidenceFactor { Name = "Image quality", Impact = -4m, Detail = "Image quality is missing or zero." });
            score -= 4m;
        }

        if (input.UserConfirmations > 0)
        {
            var confirmationBoost = Math.Min(12m, input.UserConfirmations * 2m);
            score += confirmationBoost;
            supporting.Add(new ConfidenceFactor { Name = "User confirmations", Impact = confirmationBoost, Detail = $"{input.UserConfirmations} user confirmation(s)." });
        }

        if (input.MarketplaceSupportCount > 0)
        {
            var marketplaceBoost = Math.Min(10m, input.MarketplaceSupportCount * 2m);
            score += marketplaceBoost;
            supporting.Add(new ConfidenceFactor { Name = "Marketplace support", Impact = marketplaceBoost, Detail = $"{input.MarketplaceSupportCount} supporting market source(s)." });
        }

        if (input.SourceFreshnessDays > 90)
        {
            score -= 6m;
            contradicting.Add(new ConfidenceFactor { Name = "Source freshness", Impact = -6m, Detail = "Sources are older than 90 days." });
        }

        if (input.ContradictingEvidenceCount > 0)
        {
            var contradictionPenalty = Math.Min(30m, input.ContradictingEvidenceCount * 8m);
            score -= contradictionPenalty;
            contradicting.Add(new ConfidenceFactor { Name = "Contradicting evidence", Impact = -contradictionPenalty, Detail = $"{input.ContradictingEvidenceCount} contradicting evidence record(s)." });
        }

        if (input.UserCorrections > 0)
        {
            var correctionPenalty = Math.Min(20m, input.UserCorrections * 6m);
            score -= correctionPenalty;
            contradicting.Add(new ConfidenceFactor { Name = "User corrections", Impact = -correctionPenalty, Detail = $"{input.UserCorrections} correction(s) reduced confidence." });
        }

        if (input.MissingRequiredFieldCount > 0)
        {
            var missingPenalty = Math.Min(20m, input.MissingRequiredFieldCount * 4m);
            score -= missingPenalty;
            missing.Add(new ConfidenceFactor { Name = "Missing required fields", Impact = -missingPenalty, Detail = $"{input.MissingRequiredFieldCount} required field(s) missing." });
        }

        if (!input.MasterCatalogExactMatch && !input.StableIdMatch && input.UserConfirmations < 2 && input.MarketplaceSupportCount < 2)
        {
            score = Math.Min(score, 69m);
            missing.Add(new ConfidenceFactor
            {
                Name = "Insufficient independent support",
                Impact = -12m,
                Detail = "High confidence is capped without catalog/stable ID or multi-source support."
            });
        }

        score = decimal.Clamp(score, 0m, 100m);
        var classification = Classify(score, input);

        return new ConfidenceScoreResult
        {
            Score = score,
            Classification = classification,
            SupportingFactors = supporting,
            ContradictingFactors = contradicting,
            MissingDataFactors = missing,
            RuleVersion = KnowledgeModelVersionService.ConfidenceRuleVersionValue,
            CalculatedAt = DateTimeOffset.UtcNow
        };

        void AddSupport(bool condition, decimal impact, string name, string detail)
        {
            if (!condition) return;
            score += impact;
            supporting.Add(new ConfidenceFactor { Name = name, Impact = impact, Detail = detail });
        }
    }

    private static ConfidenceClassification Classify(decimal score, ConfidenceScoringInput input)
    {
        if (input.MissingRequiredFieldCount >= 3 && score < 40m)
        {
            return ConfidenceClassification.InsufficientEvidence;
        }

        if (score >= 95m && input.ContradictingEvidenceCount == 0 && input.MissingRequiredFieldCount == 0)
        {
            return ConfidenceClassification.Verified;
        }

        if (score >= 85m) return ConfidenceClassification.VeryHigh;
        if (score >= 70m) return ConfidenceClassification.High;
        if (score >= 50m) return ConfidenceClassification.Medium;
        if (score >= 30m) return ConfidenceClassification.Low;
        return ConfidenceClassification.VeryLow;
    }
}

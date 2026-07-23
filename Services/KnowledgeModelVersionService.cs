using IBSCardManager.Models;
using IBSCardManager.Options;
using Microsoft.Extensions.Options;

namespace IBSCardManager.Services;

public sealed class KnowledgeModelVersionService : IKnowledgeModelVersionService
{
    public const string ConfidenceRuleVersionValue = "knowledge-confidence-rules-v1";
    public const string KnowledgeSchemaVersionValue = "knowledge-schema-v1";
    public const string LearningRuleVersionValue = "knowledge-learning-rules-v1";
    public const string PromptTemplateVersionValue = "scanner-prompt-v1";

    private readonly OpenAiCardAnalysisOptions _openAiOptions;

    public KnowledgeModelVersionService(IOptions<OpenAiCardAnalysisOptions> openAiOptions)
    {
        _openAiOptions = openAiOptions.Value;
    }

    public KnowledgeVersionInfo GetCurrentVersions()
    {
        return new KnowledgeVersionInfo
        {
            AiModelName = string.IsNullOrWhiteSpace(_openAiOptions.VisionModel) ? null : _openAiOptions.VisionModel,
            AiModelVersion = null,
            PromptTemplateVersion = PromptTemplateVersionValue,
            ConfidenceRuleVersion = ConfidenceRuleVersionValue,
            KnowledgeSchemaVersion = KnowledgeSchemaVersionValue,
            LearningRuleVersion = LearningRuleVersionValue
        };
    }
}

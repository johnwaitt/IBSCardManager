namespace IBSCardManager.Options;

public sealed class OpenAiCardAnalysisOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string VisionModel { get; set; } = "gpt-4.1-mini";
    public int TimeoutSeconds { get; set; } = 60;
    public int MaxRetries { get; set; } = 2;
    public int MaxImageBytes { get; set; } = 8 * 1024 * 1024;
    public bool ReuseCachedAnalysis { get; set; } = true;
    public bool EnableChatGptAnalysis { get; set; } = true;
    public bool AskBeforeUpload { get; set; } = true;
    public bool AllowTextOnlyOnlineSearch { get; set; } = true;
    public bool LocalAnalysisOnly { get; set; }

    public bool EnableWebLookup { get; set; } = true;
    public bool AskBeforeWebLookup { get; set; } = true;
    public decimal LocalConfidenceThresholdPercent { get; set; } = 85m;
    public string SearchProvider { get; set; } = "None";
    public string SearchProviderApiKey { get; set; } = string.Empty;
    public string? SearchProviderEndpoint { get; set; }
    public bool RememberLastConsentChoice { get; set; } = true;
}

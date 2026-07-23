namespace IBSCardManager.Models;

public sealed class AdministrationSecurityViewModel
{
    public string OpenAiApiKeyMasked { get; init; } = "Not configured";
    public string OpenAiProjectId { get; init; } = string.Empty;
    public string OpenAiOrganizationId { get; init; } = string.Empty;
    public bool OpenAiEnabled { get; init; }
    public string OpenAiDefaultModel { get; init; } = "gpt-4.1-mini";
    public string OpenAiVisionModel { get; init; } = "gpt-4.1-mini";
    public decimal OpenAiTemperature { get; init; } = 0.2m;
    public int OpenAiMaximumTokens { get; init; } = 1000;
    public int OpenAiTimeoutSeconds { get; init; } = 60;
    public int OpenAiRetryCount { get; init; } = 2;
    public bool OpenAiVisionEnabled { get; init; }
    public bool OpenAiOcrEnabled { get; init; }
    public bool OpenAiCardIdentificationEnabled { get; init; }
    public bool OpenAiLearningEnabled { get; init; }
    public bool OpenAiUsageStatisticsEnabled { get; init; }
    public decimal OpenAiEstimatedCost { get; init; }
    public DateTimeOffset? OpenAiLastSuccessfulRequestUtc { get; init; }
    public string EBayClientIdMasked { get; init; } = "Not configured";
    public string EBayRefreshTokenMasked { get; init; } = "Not configured";
    public bool EncryptionAvailable { get; init; }
    public string CredentialVaultStatus { get; init; } = "Unknown";
}

public sealed class OpenAiCredentialUpdateInput
{
    public string ApiKey { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
}

public sealed class OpenAiSettingsUpdateInput
{
    public bool Enabled { get; set; }
    public string DefaultModel { get; set; } = "gpt-4.1-mini";
    public string VisionModel { get; set; } = "gpt-4.1-mini";
    public decimal Temperature { get; set; } = 0.2m;
    public int MaximumTokens { get; set; } = 1000;
    public int TimeoutSeconds { get; set; } = 60;
    public int RetryCount { get; set; } = 2;
    public bool EnableVision { get; set; }
    public bool EnableOcr { get; set; }
    public bool EnableCardIdentification { get; set; }
    public bool EnableAiLearning { get; set; }
    public bool EnableUsageStatistics { get; set; }
}

public sealed class EBayCredentialUpdateInput
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

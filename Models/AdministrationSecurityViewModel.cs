namespace IBSCardManager.Models;

public sealed class AdministrationSecurityViewModel
{
    public string OpenAiApiKeyMasked { get; init; } = "Not configured";
    public string OpenAiProjectId { get; init; } = string.Empty;
    public string OpenAiOrganizationId { get; init; } = string.Empty;
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

public sealed class EBayCredentialUpdateInput
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

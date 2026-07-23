using IBSCardManager.Models;
using IBSCardManager.Options;
using IBSCardManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IBSCardManager.Controllers;

public sealed class AdministrationController : Controller
{
    private readonly IAdministrationCenterService _administrationCenterService;
    private readonly IProviderManager _providerManager;
    private readonly ICredentialVaultService _credentialVault;
    private readonly OpenAiProviderOptions _openAiProviderOptions;
    private readonly OpenAiCardAnalysisOptions _openAiCardAnalysisOptions;
    private readonly MarketplaceProviderOptions _marketplaceProviderOptions;
    private readonly IAdministrationAuditService _auditService;
    private readonly IFeatureFlagService _featureFlagService;
    private readonly IKnowledgeModelVersionService _knowledgeModelVersionService;
    private readonly IDiagnosticsService _diagnosticsService;

    public AdministrationController(
        IAdministrationCenterService administrationCenterService,
        IProviderManager providerManager,
        ICredentialVaultService credentialVault,
        IOptions<OpenAiProviderOptions> openAiProviderOptions,
        IOptions<OpenAiCardAnalysisOptions> openAiCardAnalysisOptions,
        IOptions<MarketplaceProviderOptions> marketplaceProviderOptions,
        IAdministrationAuditService auditService,
        IFeatureFlagService featureFlagService,
        IKnowledgeModelVersionService knowledgeModelVersionService,
        IDiagnosticsService diagnosticsService)
    {
        _administrationCenterService = administrationCenterService;
        _providerManager = providerManager;
        _credentialVault = credentialVault;
        _openAiProviderOptions = openAiProviderOptions.Value;
        _openAiCardAnalysisOptions = openAiCardAnalysisOptions.Value;
        _marketplaceProviderOptions = marketplaceProviderOptions.Value;
        _auditService = auditService;
        _featureFlagService = featureFlagService;
        _knowledgeModelVersionService = knowledgeModelVersionService;
        _diagnosticsService = diagnosticsService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _administrationCenterService.BuildDashboardAsync(cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Connections(CancellationToken cancellationToken)
    {
        var providers = await _providerManager.GetAllProvidersAsync(cancellationToken);
        return View(new ProviderDashboardViewModel
        {
            Providers = providers,
            GeneratedAt = DateTimeOffset.UtcNow
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestConnection(string providerName, CancellationToken cancellationToken)
    {
        var result = await _providerManager.TestConnectionAsync(providerName, cancellationToken);
        _auditService.Record("Provider Changes", "Test Connection", $"{providerName}: {(result.Success ? "Success" : "Failure")} - {result.Message}");
        TempData["ConnectionsMessage"] = $"{providerName}: {result.Message}";
        return RedirectToAction(nameof(Connections));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestOpenAiConnection(CancellationToken cancellationToken)
    {
        var result = await _providerManager.TestConnectionAsync("OpenAI", cancellationToken);
        _auditService.Record("Provider Changes", "Test OpenAI Connection", $"OpenAI: {(result.Success ? "Success" : "Failure")} - {result.Message}");
        TempData["SecurityMessage"] = result.Success
            ? "OpenAI connection succeeded. The scanner can now analyze accepted card images."
            : $"OpenAI connection failed: {result.Message}";
        return RedirectToAction(nameof(Security));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnableProvider(string providerName, CancellationToken cancellationToken)
    {
        var ok = await _providerManager.EnableProviderAsync(providerName, cancellationToken);
        _auditService.Record("Provider Changes", "Enable", ok ? $"Enabled {providerName}." : $"Enable failed for {providerName}.");
        TempData["ConnectionsMessage"] = ok ? $"{providerName} enabled." : $"{providerName} not found.";
        return RedirectToAction(nameof(Connections));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DisableProvider(string providerName, CancellationToken cancellationToken)
    {
        var ok = await _providerManager.DisableProviderAsync(providerName, cancellationToken);
        _auditService.Record("Provider Changes", "Disable", ok ? $"Disabled {providerName}." : $"Disable failed for {providerName}.");
        TempData["ConnectionsMessage"] = ok ? $"{providerName} disabled." : $"{providerName} not found.";
        return RedirectToAction(nameof(Connections));
    }

    [HttpGet]
    public IActionResult MasterDatabase() => View();

    [HttpGet]
    public IActionResult Security()
    {
        var model = new AdministrationSecurityViewModel
        {
            OpenAiApiKeyMasked = _credentialVault.Mask(_openAiProviderOptions.ApiKeyEncrypted),
            OpenAiProjectId = string.IsNullOrWhiteSpace(_openAiProviderOptions.ProjectId) ? "Not configured" : _openAiProviderOptions.ProjectId,
            OpenAiOrganizationId = string.IsNullOrWhiteSpace(_openAiProviderOptions.OrganizationId) ? "Not configured" : _openAiProviderOptions.OrganizationId,
            EBayClientIdMasked = _credentialVault.Mask(_marketplaceProviderOptions.EBay.ClientIdEncrypted),
            EBayRefreshTokenMasked = _credentialVault.Mask(_marketplaceProviderOptions.EBay.RefreshTokenEncrypted),
            EncryptionAvailable = true,
            CredentialVaultStatus = "Enabled (CurrentUser protected storage)"
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveOpenAiCredentials(OpenAiCredentialUpdateInput input)
    {
        if (!string.IsNullOrWhiteSpace(input.ApiKey))
        {
            var apiKey = input.ApiKey.Trim();
            _openAiProviderOptions.ApiKeyEncrypted = _credentialVault.Protect(apiKey);
            _openAiCardAnalysisOptions.ApiKey = apiKey;
        }

        _openAiProviderOptions.ProjectId = input.ProjectId?.Trim() ?? string.Empty;
        _openAiProviderOptions.OrganizationId = input.OrganizationId?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(_openAiProviderOptions.VisionModel))
        {
            _openAiCardAnalysisOptions.VisionModel = _openAiProviderOptions.VisionModel;
        }

        _openAiCardAnalysisOptions.EnableChatGptAnalysis = _openAiProviderOptions.Enabled;
        _openAiCardAnalysisOptions.LocalAnalysisOnly = false;

        _auditService.Record("Credential Updates", "OpenAI", "OpenAI credentials were updated and connected to scanner image analysis for this runtime session.");
        TempData["SecurityMessage"] = "OpenAI credentials saved securely and connected to the scanner. Use Test OpenAI Connection to verify access.";
        return RedirectToAction(nameof(Security));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveEbayCredentials(EBayCredentialUpdateInput input)
    {
        if (!string.IsNullOrWhiteSpace(input.ClientId))
        {
            _marketplaceProviderOptions.EBay.ClientIdEncrypted = _credentialVault.Protect(input.ClientId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(input.ClientSecret))
        {
            _marketplaceProviderOptions.EBay.ClientSecretEncrypted = _credentialVault.Protect(input.ClientSecret.Trim());
        }

        if (!string.IsNullOrWhiteSpace(input.RefreshToken))
        {
            _marketplaceProviderOptions.EBay.RefreshTokenEncrypted = _credentialVault.Protect(input.RefreshToken.Trim());
        }

        _auditService.Record("Credential Updates", "eBay", "eBay credentials were updated through Administration/Security.");
        TempData["SecurityMessage"] = "eBay credentials updated in secure vault memory for this runtime session.";
        return RedirectToAction(nameof(Security));
    }

    [HttpGet]
    public IActionResult Diagnostics() => RedirectToAction("Diagnostics", "Home");

    [HttpGet]
    public IActionResult BackgroundJobs()
    {
        var model = new AdministrationBackgroundJobsViewModel
        {
            Queues = new[]
            {
                new AdministrationQueueStatusViewModel { QueueName = "AI Queue", Status = "Waiting", Running = 0, Waiting = 0, Failed = 0, Completed = 0, RetrySupported = true },
                new AdministrationQueueStatusViewModel { QueueName = "Scanner Queue", Status = "Waiting", Running = 0, Waiting = 0, Failed = 0, Completed = 0, RetrySupported = true },
                new AdministrationQueueStatusViewModel { QueueName = "Marketplace Queue", Status = "Waiting", Running = 0, Waiting = 0, Failed = 0, Completed = 0, RetrySupported = true },
                new AdministrationQueueStatusViewModel { QueueName = "Pricing Queue", Status = "Waiting", Running = 0, Waiting = 0, Failed = 0, Completed = 0, RetrySupported = true },
                new AdministrationQueueStatusViewModel { QueueName = "Image Queue", Status = "Waiting", Running = 0, Waiting = 0, Failed = 0, Completed = 0, RetrySupported = true },
                new AdministrationQueueStatusViewModel { QueueName = "Backup Queue", Status = "Waiting", Running = 0, Waiting = 0, Failed = 0, Completed = 0, RetrySupported = true },
                new AdministrationQueueStatusViewModel { QueueName = "Catalog Queue", Status = "Waiting", Running = 0, Waiting = 0, Failed = 0, Completed = 0, RetrySupported = true }
            }
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Backups() => RedirectToAction("BackupManifest", "Home");

    [HttpGet]
    public async Task<IActionResult> Updates(CancellationToken cancellationToken)
    {
        var diagnostics = await _diagnosticsService.BuildDiagnosticsAsync(cancellationToken);
        var knowledgeVersions = _knowledgeModelVersionService.GetCurrentVersions();

        return View(new AdministrationUpdatesViewModel
        {
            ApplicationVersion = diagnostics.ApplicationVersion,
            InformationalVersion = diagnostics.InformationalVersion,
            DatabaseVersion = diagnostics.SchemaVersion,
            CatalogVersion = diagnostics.CatalogVersion,
            KnowledgeVersion = diagnostics.KnowledgeSchemaVersion,
            PromptVersion = knowledgeVersions.PromptTemplateVersion,
            RuleVersion = knowledgeVersions.ConfidenceRuleVersion,
            MarketplaceVersion = "provider-platform-v1",
            AiModelVersion = knowledgeVersions.AiModelName
        });
    }

    [HttpGet]
    public IActionResult Developer()
    {
        if (!_featureFlagService.IsDeveloperModeEnabled())
        {
            TempData["DeveloperModeMessage"] = "Developer mode is currently disabled.";
            return RedirectToAction(nameof(Index));
        }

        return View();
    }

    [HttpGet]
    public IActionResult SystemSettings() => View();

    [HttpGet]
    public IActionResult AuditHistory()
    {
        return View(new AdministrationAuditHistoryViewModel
        {
            Entries = _auditService.GetRecent()
        });
    }

    [HttpGet]
    public IActionResult FeatureManager()
    {
        return View(new AdministrationFeatureManagerViewModel
        {
            DeveloperModeEnabled = _featureFlagService.IsDeveloperModeEnabled(),
            FeatureFlags = _featureFlagService.GetAll()
        });
    }
}

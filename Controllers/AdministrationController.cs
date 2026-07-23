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
            OpenAiEnabled = _openAiProviderOptions.Enabled && _openAiCardAnalysisOptions.EnableChatGptAnalysis,
            OpenAiDefaultModel = _openAiProviderOptions.DefaultModel,
            OpenAiVisionModel = _openAiProviderOptions.VisionModel,
            OpenAiTemperature = _openAiProviderOptions.Temperature,
            OpenAiMaximumTokens = _openAiProviderOptions.MaximumTokens,
            OpenAiTimeoutSeconds = _openAiProviderOptions.TimeoutSeconds,
            OpenAiRetryCount = _openAiProviderOptions.RetryCount,
            OpenAiVisionEnabled = _openAiProviderOptions.EnableVision,
            OpenAiOcrEnabled = _openAiProviderOptions.EnableOcr,
            OpenAiCardIdentificationEnabled = _openAiProviderOptions.EnableCardIdentification,
            OpenAiLearningEnabled = _openAiProviderOptions.EnableAiLearning,
            OpenAiUsageStatisticsEnabled = _openAiProviderOptions.EnableUsageStatistics,
            OpenAiEstimatedCost = _openAiProviderOptions.EstimatedCost,
            OpenAiLastSuccessfulRequestUtc = _openAiProviderOptions.LastSuccessfulRequestUtc,
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
        _openAiProviderOptions.Enabled = true;

        if (!string.IsNullOrWhiteSpace(_openAiProviderOptions.VisionModel))
        {
            _openAiCardAnalysisOptions.VisionModel = _openAiProviderOptions.VisionModel;
        }

        _openAiCardAnalysisOptions.EnableChatGptAnalysis = true;
        _openAiCardAnalysisOptions.LocalAnalysisOnly = false;

        _auditService.Record("Credential Updates", "OpenAI Save", "OpenAI credentials were added or edited and connected to scanner image analysis for this runtime session.");
        TempData["SecurityMessage"] = "OpenAI credentials saved securely and connected to the scanner. Use Test OpenAI Connection to verify access.";
        return RedirectToAction(nameof(Security));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConnectOpenAi()
    {
        if (string.IsNullOrWhiteSpace(_openAiProviderOptions.ApiKeyEncrypted) && string.IsNullOrWhiteSpace(_openAiCardAnalysisOptions.ApiKey))
        {
            TempData["SecurityMessage"] = "Add and save an OpenAI API key before connecting.";
            return RedirectToAction(nameof(Security));
        }

        if (string.IsNullOrWhiteSpace(_openAiCardAnalysisOptions.ApiKey) && !string.IsNullOrWhiteSpace(_openAiProviderOptions.ApiKeyEncrypted))
        {
            try
            {
                _openAiCardAnalysisOptions.ApiKey = _credentialVault.Unprotect(_openAiProviderOptions.ApiKeyEncrypted);
            }
            catch
            {
                TempData["SecurityMessage"] = "The protected OpenAI key could not be opened for the current Windows user. Edit and save the key again.";
                return RedirectToAction(nameof(Security));
            }
        }

        _openAiProviderOptions.Enabled = true;
        _openAiCardAnalysisOptions.EnableChatGptAnalysis = true;
        _openAiCardAnalysisOptions.LocalAnalysisOnly = false;
        _auditService.Record("Provider Changes", "OpenAI Connect", "OpenAI was connected to scanner analysis.");
        TempData["SecurityMessage"] = "OpenAI connected. Run Test OpenAI Connection to verify live access.";
        return RedirectToAction(nameof(Security));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DisconnectOpenAi()
    {
        _openAiProviderOptions.Enabled = false;
        _openAiCardAnalysisOptions.EnableChatGptAnalysis = false;
        _openAiCardAnalysisOptions.LocalAnalysisOnly = true;
        _auditService.Record("Provider Changes", "OpenAI Disconnect", "OpenAI was disconnected. Protected credentials were retained.");
        TempData["SecurityMessage"] = "OpenAI disconnected. The protected credentials were kept and can be reconnected later.";
        return RedirectToAction(nameof(Security));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveOpenAiCredentials()
    {
        _openAiProviderOptions.Enabled = false;
        _openAiProviderOptions.ApiKeyEncrypted = string.Empty;
        _openAiProviderOptions.ProjectId = string.Empty;
        _openAiProviderOptions.OrganizationId = string.Empty;
        _openAiProviderOptions.LastSuccessfulRequestUtc = null;
        _openAiCardAnalysisOptions.ApiKey = string.Empty;
        _openAiCardAnalysisOptions.EnableChatGptAnalysis = false;
        _openAiCardAnalysisOptions.LocalAnalysisOnly = true;

        _auditService.Record("Credential Updates", "OpenAI Remove", "OpenAI credentials were removed and the provider was disconnected.");
        TempData["SecurityMessage"] = "OpenAI credentials removed. Add a new API key to connect again.";
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

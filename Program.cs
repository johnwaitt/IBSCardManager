using IBSCardManager.Data;
using IBSCardManager.Options;
using Microsoft.EntityFrameworkCore;
using IBSCardManager.Services;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.Configure<OpenAiCardAnalysisOptions>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<OpenAiProviderOptions>(builder.Configuration.GetSection("Providers:AI:OpenAI"));
builder.Services.Configure<MarketplaceProviderOptions>(builder.Configuration.GetSection("Providers:Marketplace"));
builder.Services.Configure<FeatureManagementOptions>(builder.Configuration.GetSection("FeatureManagement"));
builder.Services.AddHttpClient<IOpenAiCardAnalysisService, OpenAiCardAnalysisService>();
builder.Services.AddScoped<ICardImageIdentificationService, CardImageIdentificationService>();
builder.Services.AddScoped<ICardImageAnalysisService, CardImageIdentificationService>();
builder.Services.AddScoped<ICardMetadataExtractionService, CardImageIdentificationService>();
builder.Services.AddScoped<ICardCandidateMatchingService, CardImageIdentificationService>();
builder.Services.AddScoped<ICardWebSearchService, NoOpCardWebSearchService>();
builder.Services.AddScoped<IChecklistCandidateService, ChecklistCandidateService>();
builder.Services.AddScoped<IReferenceImageService, ReferenceImageService>();
builder.Services.AddScoped<IMasterCatalogService, LocalMasterCatalogService>();
builder.Services.AddScoped<ICatalogLookupService, LocalCatalogLookupService>();
builder.Services.AddScoped<ICatalogSearchService, LocalCatalogSearchService>();
builder.Services.AddScoped<ICatalogVersionService, LocalCatalogVersionService>();
builder.Services.AddScoped<ICatalogImportService, LocalCatalogImportService>();
builder.Services.AddScoped<ICatalogImageService, LocalCatalogImageService>();
builder.Services.AddScoped<ICatalogDatabaseProvider, LocalCatalogDatabaseProvider>();
builder.Services.AddScoped<ICatalogValidationService, CatalogValidationService>();
builder.Services.AddSingleton<IApplicationVersionProvider, ApplicationVersionProvider>();
builder.Services.AddScoped<IKnowledgeService, KnowledgeService>();
builder.Services.AddScoped<IKnowledgeQueryService, KnowledgeQueryService>();
builder.Services.AddScoped<IKnowledgeLearningService, KnowledgeLearningService>();
builder.Services.AddScoped<IKnowledgeEvidenceService, KnowledgeEvidenceService>();
builder.Services.AddScoped<IKnowledgeCorrectionService, KnowledgeCorrectionService>();
builder.Services.AddScoped<IConfidenceScoringService, ConfidenceScoringService>();
builder.Services.AddScoped<IDecisionHistoryService, DecisionHistoryService>();
builder.Services.AddScoped<IKnowledgeModelVersionService, KnowledgeModelVersionService>();
builder.Services.AddScoped<IKnowledgeHealthService, KnowledgeHealthService>();
builder.Services.AddScoped<IDiagnosticsService, DiagnosticsService>();
builder.Services.AddScoped<IBackupManifestService, BackupManifestService>();
builder.Services.AddScoped<IAdministrationCenterService, AdministrationCenterService>();
builder.Services.AddSingleton<ICredentialVaultService, CredentialVaultService>();
builder.Services.AddSingleton<IFeatureFlagService, FeatureFlagService>();
builder.Services.AddSingleton<IAdministrationAuditService, AdministrationAuditService>();
builder.Services.AddScoped<IAIProvider, OpenAiProvider>();
builder.Services.AddScoped<IMarketplaceProvider, EBayMarketplaceProvider>();
builder.Services.AddScoped<IMarketplaceProvider, WhatnotMarketplaceProvider>();
builder.Services.AddScoped<IProviderBase>(sp => sp.GetRequiredService<IAIProvider>());
builder.Services.AddScoped<IProviderBase>(sp => sp.GetServices<IMarketplaceProvider>().First());
builder.Services.AddScoped<IProviderBase>(sp => sp.GetServices<IMarketplaceProvider>().Skip(1).First());
builder.Services.AddScoped<IProviderBase>(_ => new PlaceholderProvider("Azure OpenAI", "AI"));
builder.Services.AddScoped<IProviderBase>(_ => new PlaceholderProvider("Claude", "AI"));
builder.Services.AddScoped<IProviderBase>(_ => new PlaceholderProvider("Gemini", "AI"));
builder.Services.AddScoped<IProviderBase>(_ => new PlaceholderProvider("Ollama", "AI"));
builder.Services.AddScoped<IProviderBase>(_ => new PlaceholderProvider("LM Studio", "AI"));
builder.Services.AddScoped<IProviderBase>(_ => new PlaceholderProvider("COMC", "Marketplace"));
builder.Services.AddScoped<IProviderBase>(_ => new PlaceholderProvider("PWCC", "Marketplace"));
builder.Services.AddScoped<IProviderBase>(_ => new PlaceholderProvider("Goldin", "Marketplace"));
builder.Services.AddScoped<IProviderBase>(_ => new PlaceholderProvider("Fanatics Collect", "Marketplace"));
builder.Services.AddScoped<IProviderBase>(_ => new PlaceholderProvider("CollX", "Marketplace"));
builder.Services.AddScoped<IProviderBase>(_ => new PlaceholderProvider("TCDB", "Marketplace"));
builder.Services.AddScoped<IProviderManager, ProviderManager>();
builder.Services.AddMemoryCache();
builder.Services.Configure<DashboardOptions>(builder.Configuration.GetSection("DashboardCache"));
builder.Services.AddScoped<ICollectionAnalyticsService, CollectionAnalyticsService>();
builder.Services.AddScoped<ICollectionInsightsService, CollectionInsightsService>();
builder.Services.AddSingleton<AnalyticsRecalculationQueue>();
builder.Services.AddSingleton<IAnalyticsRecalculationQueue>(sp => sp.GetRequiredService<AnalyticsRecalculationQueue>());
builder.Services.AddHostedService<AnalyticsRecalculationWorker>();

var connectionString =
    builder.Configuration.GetConnectionString(
        "CardManagerConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'CardManagerConnection' was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseSqlServer(connectionString)
            .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await DatabaseSeeder.SeedAsync(context);
}

app.Run();

using IBSCardManager.Models;
using IBSCardManager.Services;
using Xunit;

namespace IBSCardManager.Tests;

public sealed class ProviderManagerTests
{
    [Fact]
    public async Task ProviderManager_Returns_Registered_Providers()
    {
        var manager = new ProviderManager(new IProviderBase[]
        {
            new TestProvider("OpenAI", "AI", ProviderReadiness.ProductionReady),
            new TestProvider("Whatnot", "Marketplace", ProviderReadiness.Limited)
        });

        var providers = await manager.GetAllProvidersAsync();
        Assert.Equal(2, providers.Count);
        Assert.Contains(providers, p => p.ProviderName == "OpenAI");
        Assert.Contains(providers, p => p.ProviderName == "Whatnot" && p.Readiness == ProviderReadiness.Limited);
    }

    [Fact]
    public async Task ProviderManager_TestConnection_Returns_Result()
    {
        var manager = new ProviderManager(new IProviderBase[] { new TestProvider("OpenAI", "AI", ProviderReadiness.ProductionReady) });
        var result = await manager.TestConnectionAsync("OpenAI");

        Assert.True(result.Success);
        Assert.Equal("OpenAI", result.ProviderName);
    }

    [Fact]
    public async Task ProviderManager_UnknownProvider_Returns_NotFound_Result()
    {
        var manager = new ProviderManager(Array.Empty<IProviderBase>());
        var result = await manager.TestConnectionAsync("Missing");

        Assert.False(result.Success);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class TestProvider : IProviderBase
    {
        public TestProvider(string name, string type, ProviderReadiness readiness)
        {
            Name = name;
            ProviderType = type;
            Readiness = readiness;
        }

        public string Name { get; }
        public string ProviderType { get; }
        public string Version => "v1";
        public ProviderReadiness Readiness { get; }
        public bool IsEnabled => true;

        public Task<ProviderStatusRecord> GetStatusAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new ProviderStatusRecord
            {
                ProviderName = Name,
                ProviderType = ProviderType,
                Enabled = true,
                Status = "Configured",
                Version = Version,
                Readiness = Readiness
            });

        public Task<ProviderConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new ProviderConnectionTestResult
            {
                ProviderName = Name,
                Success = true,
                Message = "ok",
                TestedAt = DateTimeOffset.UtcNow
            });
    }
}

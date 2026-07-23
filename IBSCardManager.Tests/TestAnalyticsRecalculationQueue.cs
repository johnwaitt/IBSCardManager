using IBSCardManager.Services;

namespace IBSCardManager.Tests;

public sealed class TestAnalyticsRecalculationQueue : IAnalyticsRecalculationQueue
{
    public List<string> Triggers { get; } = new();

    public Task EnqueueAsync(string trigger, CancellationToken cancellationToken = default)
    {
        Triggers.Add(trigger);
        return Task.CompletedTask;
    }
}

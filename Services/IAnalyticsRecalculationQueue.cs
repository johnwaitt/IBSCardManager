namespace IBSCardManager.Services;

public interface IAnalyticsRecalculationQueue
{
    Task EnqueueAsync(string trigger, CancellationToken cancellationToken = default);
}

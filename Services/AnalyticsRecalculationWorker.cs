using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IBSCardManager.Services;

public sealed class AnalyticsRecalculationWorker : BackgroundService
{
    private readonly AnalyticsRecalculationQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AnalyticsRecalculationWorker> _logger;

    public AnalyticsRecalculationWorker(AnalyticsRecalculationQueue queue, IServiceProvider serviceProvider, ILogger<AnalyticsRecalculationWorker> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var trigger in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var insights = scope.ServiceProvider.GetRequiredService<ICollectionInsightsService>();
                await insights.RecalculateAnalyticsAsync(trigger, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analytics recalculation failed for trigger {Trigger}", trigger);
            }
        }
    }
}

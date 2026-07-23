using System.Threading.Channels;

namespace IBSCardManager.Services;

public sealed class AnalyticsRecalculationQueue : IAnalyticsRecalculationQueue
{
    private readonly Channel<string> _queue;

    public AnalyticsRecalculationQueue()
    {
        _queue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public async Task EnqueueAsync(string trigger, CancellationToken cancellationToken = default)
    {
        await _queue.Writer.WriteAsync(trigger, cancellationToken);
    }

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        return _queue.Reader.ReadAllAsync(cancellationToken);
    }
}

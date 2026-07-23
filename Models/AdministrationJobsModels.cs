namespace IBSCardManager.Models;

public sealed class AdministrationQueueStatusViewModel
{
    public string QueueName { get; init; } = string.Empty;
    public string Status { get; init; } = "Unknown";
    public int Running { get; init; }
    public int Waiting { get; init; }
    public int Failed { get; init; }
    public int Completed { get; init; }
    public bool RetrySupported { get; init; }
}

public sealed class AdministrationBackgroundJobsViewModel
{
    public IReadOnlyList<AdministrationQueueStatusViewModel> Queues { get; init; } = Array.Empty<AdministrationQueueStatusViewModel>();
}

using IBSCardManager.Models;

namespace IBSCardManager.Services;

public interface IAdministrationAuditService
{
    void Record(string category, string action, string detail);
    IReadOnlyList<AdministrationAuditEntryViewModel> GetRecent(int take = 200);
}

public sealed class AdministrationAuditService : IAdministrationAuditService
{
    private static readonly object Sync = new();
    private static readonly List<AdministrationAuditEntryViewModel> Entries = new();

    public void Record(string category, string action, string detail)
    {
        lock (Sync)
        {
            Entries.Add(new AdministrationAuditEntryViewModel
            {
                Timestamp = DateTimeOffset.UtcNow,
                Category = category,
                Action = action,
                Detail = detail
            });

            if (Entries.Count > 1000)
            {
                Entries.RemoveRange(0, Entries.Count - 1000);
            }
        }
    }

    public IReadOnlyList<AdministrationAuditEntryViewModel> GetRecent(int take = 200)
    {
        lock (Sync)
        {
            return Entries.OrderByDescending(x => x.Timestamp).Take(Math.Clamp(take, 1, 1000)).ToList();
        }
    }
}

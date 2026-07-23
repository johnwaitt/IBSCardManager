using IBSCardManager.Models;

namespace IBSCardManager.Services;

public interface IBackupManifestService
{
    Task<BackupManifest> GenerateManifestAsync(CancellationToken cancellationToken = default);
}

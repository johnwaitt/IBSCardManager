using IBSCardManager.Models;

namespace IBSCardManager.Services;

public interface IReferenceImageService
{
    Task<ReferenceImageMetadataViewModel> BuildMetadataAsync(ReferenceImageMetadataRequest request, CancellationToken cancellationToken = default);
    Task<string> ResolveDisplayUrlAsync(ReferenceImageMetadataViewModel metadata, CancellationToken cancellationToken = default);
}

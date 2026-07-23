using IBSCardManager.Models;

namespace IBSCardManager.Services;

public sealed class ReferenceImageService : IReferenceImageService
{
    public Task<ReferenceImageMetadataViewModel> BuildMetadataAsync(ReferenceImageMetadataRequest request, CancellationToken cancellationToken = default)
    {
        var metadata = new ReferenceImageMetadataViewModel
        {
            ReferenceImageUrl = request.ReferenceImageUrl,
            ReferencePageUrl = request.ReferencePageUrl,
            ImageSource = request.ImageSource,
            DateLocatedUtc = request.DateLocatedUtc ?? DateTime.UtcNow,
            UsageStatus = string.IsNullOrWhiteSpace(request.UsageStatus) ? "Unknown" : request.UsageStatus,
            CachedThumbnailPath = request.CachedThumbnailPath,
            ImageHash = request.ImageHash,
            VerificationStatus = string.IsNullOrWhiteSpace(request.VerificationStatus) ? "Unverified" : request.VerificationStatus
        };

        return Task.FromResult(metadata);
    }

    public Task<string> ResolveDisplayUrlAsync(ReferenceImageMetadataViewModel metadata, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(string.IsNullOrWhiteSpace(metadata.ReferenceImageUrl) ? string.Empty : metadata.ReferenceImageUrl);
    }
}

using IBSCardManager.Models;

namespace IBSCardManager.Services;

public interface IAdministrationCenterService
{
    Task<AdministrationDashboardViewModel> BuildDashboardAsync(CancellationToken cancellationToken = default);
}

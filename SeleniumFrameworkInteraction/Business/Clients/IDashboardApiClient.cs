using Business.Model.API;

namespace Business.Clients;

public interface IDashboardApiClient
{
    Task<DashboardListResponse> GetDashboardsAsync(
        string token,
        string project,
        CancellationToken cancellationToken = default);

    Task DeleteDashboardAsync(
        string token,
        string project,
        long dashboardId,
        CancellationToken cancellationToken = default);
}
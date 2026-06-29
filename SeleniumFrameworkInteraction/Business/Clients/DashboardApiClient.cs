using Business.Model.API;
using Core.Clients;

namespace Business.Clients;

public class DashboardApiClient : IDashboardApiClient
{
    private readonly IRpApiClient _rpApiClient;

    public DashboardApiClient(IRpApiClient rpApiClient)
    {
        _rpApiClient = rpApiClient;
    }

    public async Task<DashboardListResponse> GetDashboardsAsync(
        string token,
        string project,
        CancellationToken cancellationToken = default)
    {
        var request = new ApiRequest
        {
            Method = HttpMethod.Get,
            RelativeUrl = $"api/v1/{project}/dashboard?page.page=1&page.size=300",
            Headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}"
            },
            EnsureSuccessStatusCode = false
        };

        return await _rpApiClient.ExecuteAsync<DashboardListResponse>(request, cancellationToken);
    }

    public async Task DeleteDashboardAsync(
        string token,
        string project,
        long dashboardId,
        CancellationToken cancellationToken = default)
    {
        var request = new ApiRequest
        {
            Method = HttpMethod.Delete,
            RelativeUrl = $"api/v1/{project}/dashboard/{dashboardId}",
            Headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}"
            },
            EnsureSuccessStatusCode = false
        };

        await _rpApiClient.ExecuteAsync<InfoResponse>(request, cancellationToken);
    }
}
using Business.Clients;
using Business.Models;

namespace Business.Helpers;

public class DashboardCleanupApiService
{
    private readonly IAuthClient _authClient;
    private readonly IDashboardApiClient _dashboardApiClient;

    public DashboardCleanupApiService(IAuthClient authClient, IDashboardApiClient dashboardApiClient)
    {
        _authClient = authClient;
        _dashboardApiClient = dashboardApiClient;
    }

    public async Task CleanupUserTestDashboardsAsync(
        UserModel user,
        string project,
        Func<string, bool> isTestDashboard,
        CancellationToken cancellationToken = default)
    {
        var tokenResponse = await _authClient.GetTokenAsync(
            user.Login,
            user.Password,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            return;

        var dashboardsResponse = await _dashboardApiClient.GetDashboardsAsync(
            tokenResponse.AccessToken,
            project,
            cancellationToken);

        foreach (var dashboard in dashboardsResponse.Content.Where(d => isTestDashboard(d.Name)))
        {
            await _dashboardApiClient.DeleteDashboardAsync(
                tokenResponse.AccessToken,
                project,
                dashboard.Id,
                cancellationToken);
        }
    }
}
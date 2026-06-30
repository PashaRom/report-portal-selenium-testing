using Business.Clients;
using Business.Model.DTO;
using Microsoft.Extensions.Logging;

namespace Business.Services;

public class DashboardCleanupApiService
{
    private readonly IAuthClient _authClient;
    private readonly IDashboardApiClient _dashboardApiClient;
    private readonly ILogger _logger;

    public DashboardCleanupApiService(
        IAuthClient authClient,
        IDashboardApiClient dashboardApiClient,
        ILoggerFactory loggerFactory)
    {
        _authClient = authClient;
        _dashboardApiClient = dashboardApiClient;
        _logger = loggerFactory.CreateLogger<DashboardCleanupApiService>();
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
        {
            _logger.LogWarning("Could not obtain access token for user '{Login}'. Skipping cleanup.", user.Login);
            return;
        }

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
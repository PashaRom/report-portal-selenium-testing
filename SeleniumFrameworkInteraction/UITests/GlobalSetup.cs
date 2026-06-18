using Business.Data;
using Business.Clients;
using Business.Helpers;
using Core.Configuration;
using Core.Clients;
using NUnit.Framework;

[SetUpFixture]
public class GlobalSetup
{
    private static readonly Core.Clients.IRpApiClient RpApiClient = new RpApiClient();
    private static readonly IAuthClient AuthClient = new AuthClient(RpApiClient);
    private static readonly DashboardCleanupApiService DashboardCleanupApiService =
        new(AuthClient, new DashboardApiClient(RpApiClient));

    [OneTimeSetUp]
    public void PreCleanupTestDashboards()
    {
        CleanupAsync().GetAwaiter().GetResult();
    }

    [OneTimeTearDown]
    public void PostCleanupTestDashboards()
    {
        CleanupAsync().GetAwaiter().GetResult();
    }

    private static async Task CleanupAsync()
    {
        var project = AppConfiguration.ProjectName;

        foreach (var alias in TestDataProvider.LoginAliases)
        {
            var user = TestDataProvider.GetUser(alias);
            try
            {
                await DashboardCleanupApiService.CleanupUserTestDashboardsAsync(user, project, IsTestDashboard);
            }
            catch
            {
                // cleanup is best-effort; a failed user does not block others
            }
        }
    }

    private static bool IsTestDashboard(string name) =>
        name.StartsWith("DC_", StringComparison.Ordinal) ||
        name.EndsWith(" CRUD Dashboard", StringComparison.Ordinal);
}

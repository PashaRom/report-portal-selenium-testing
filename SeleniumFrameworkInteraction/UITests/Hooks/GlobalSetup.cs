using Business.Data;
using Business.DI;
using Business.Helpers;
using Core.Configuration;
using Core.DI;
using Microsoft.Extensions.Logging;

[SetUpFixture]
public class GlobalSetup
{
    private IAppConfiguration? _appConfig;
    private DashboardCleanupApiService? _cleanupService;
    private ILogger? _logger;

    [OneTimeSetUp]
    public void PreCleanupTestDashboards()
    {
        ServiceLocator.SetAdditionalRegistrations(services => services.AddBusinessServices());

        _appConfig = ServiceLocator.GetService<IAppConfiguration>();
        _cleanupService = ServiceLocator.GetService<DashboardCleanupApiService>();
        _logger = ServiceLocator.GetService<ILoggerFactory>().CreateLogger(nameof(GlobalSetup));

        CleanupAsync().GetAwaiter().GetResult();
    }

    [OneTimeTearDown]
    public void PostCleanupTestDashboards()
    {
        CleanupAsync().GetAwaiter().GetResult();
    }

    private async Task CleanupAsync()
    {
        if (_cleanupService is null || _appConfig is null || _logger is null)
        {
            return;
        }

        var project = _appConfig.ProjectName;

        foreach (var alias in TestDataProvider.LoginAliases)
        {
            var user = TestDataProvider.GetUser(alias);
            try
            {
                await _cleanupService.CleanupUserTestDashboardsAsync(user, project, IsTestDashboard);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cleanup failed for user '{Alias}', skipping", alias);
            }
        }
    }

    private static bool IsTestDashboard(string name) =>
        name.StartsWith("DC_", StringComparison.Ordinal) ||
        name.EndsWith(" CRUD Dashboard", StringComparison.Ordinal);
}

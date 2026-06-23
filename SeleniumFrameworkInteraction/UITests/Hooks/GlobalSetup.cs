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
    private UserProvisioningService? _provisioningService;
    private ILogger? _logger;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        ServiceLocator.SetAdditionalRegistrations(services => services.AddBusinessServices());

        _appConfig = ServiceLocator.GetService<IAppConfiguration>();
        _cleanupService = ServiceLocator.GetService<DashboardCleanupApiService>();
        _provisioningService = ServiceLocator.GetService<UserProvisioningService>();
        _logger = ServiceLocator.GetService<ILoggerFactory>().CreateLogger(nameof(GlobalSetup));

        ProvisionUsersAsync().GetAwaiter().GetResult();
        CleanupAsync().GetAwaiter().GetResult();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        CleanupAsync().GetAwaiter().GetResult();
    }

    private async Task ProvisionUsersAsync()
    {
        if (_provisioningService is null || _logger is null)
        {
            return;
        }

        try
        {
            _logger.LogInformation("Starting user provisioning...");
            await _provisioningService.EnsureUsersExistAsync();
            _logger.LogInformation("User provisioning complete.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "User provisioning failed, continuing with test run.");
        }
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

using Business.Steps;
using Core.Base;
using Core.DI;
using Core.Enum;
using Microsoft.Extensions.Logging;

namespace UITests.Hooks;

/// <summary>
/// Shared base for all Dashboard test fixtures.
/// Provides common <see cref="AuthSteps"/> and <see cref="DashboardSteps"/> fields,
/// a <c>[SetUp]</c> that resolves them from DI, and a <c>[TearDown]</c> that
/// deletes the dashboard created during the test (if any).
/// <para>
/// Subclasses may override <see cref="BeforeDelete"/> to insert extra steps
/// (e.g. unlocking a locked dashboard) before the final deletion.
/// </para>
/// </summary>
public abstract class DashboardTestBase : BaseTest
{
    protected AuthSteps _auth = null!;
    protected DashboardSteps _dashboard = null!;

    protected DashboardTestBase(BrowserType browser) : base(browser) { }

    [SetUp]
    public void InitSteps()
    {
        _auth = ServiceLocator.GetService<AuthSteps>();
        _dashboard = ServiceLocator.GetService<DashboardSteps>();
    }

    [TearDown]
    public void CleanupDashboard()
    {
        if (!_dashboard.HasCreatedDashboard)
        {
            Logger.LogWarning("No dashboard was created in this test, skipping cleanup");
            return;
        }

        try
        {
            _auth.LoginViaApi("default");
            _dashboard.NavigateToCreatedDashboard();
            BeforeDelete();
            _dashboard.DeleteDashboard();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Dashboard cleanup failed, skipping");
        }
    }

    /// <summary>
    /// Override to perform steps between navigation and deletion during cleanup.
    /// Default implementation is a no-op.
    /// </summary>
    protected virtual void BeforeDelete() { }
}

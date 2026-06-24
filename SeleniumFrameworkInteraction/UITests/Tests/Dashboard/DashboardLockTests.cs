using Allure.NUnit.Attributes;
using Business.Data;
using Business.Steps;
using Core.Enum;
using UITests.Hooks;

namespace UITests.Tests.Dashboard;

/// <summary>
/// Feature: Dashboard Lock
/// Verifies that Lock/Unlock buttons are available only to privileged roles.
///
/// PROJECT_MANAGER role → can lock/unlock.
/// MEMBER, OPERATOR, CUSTOMER → cannot.
/// Permission is derived from each user's ProjectsAndRoles in the CSV.
/// </summary>
[Category("dashboard_lock")]
[AllureFeature("Dashboard")]
[AllureSuite("Lock / Unlock")]
public class DashboardLockTests : DashboardTestBase
{
    public DashboardLockTests(BrowserType browser) : base(browser) { }

    /// <summary>
    /// Unlocks the dashboard before the base cleanup deletes it,
    /// in case it was left in a locked state by the test.
    /// </summary>
    protected override void BeforeDelete()
    {
        if (_dashboard.IsUnlockAvailable())
        {
            _dashboard.UnlockDashboard();
        }
    }

    [TestCaseSource(typeof(TestDataProvider), nameof(TestDataProvider.DashboardManagePermissions))]
    public void UnlockButton_Availability_ReflectsUserPermissions(string login, bool expectedAvailable)
    {
        // Setup: admin creates and locks the dashboard
        _auth.LoginViaApi("default");
        _dashboard.CreateDashboardWithUniqueName();
        _dashboard.LockDashboard();
        _auth.Logout();

        // Act: target user navigates to the locked dashboard
        _auth.LoginAs(login);
        _dashboard.NavigateToCreatedDashboard();

        Assert.That(_dashboard.IsUnlockAvailable(), Is.EqualTo(expectedAvailable),
            $"Unlock button availability for role '{login}' should be {expectedAvailable}");
    }

    [TestCaseSource(typeof(TestDataProvider), nameof(TestDataProvider.DashboardManagePermissions))]
    public void LockButton_Availability_ReflectsUserPermissions(string login, bool expectedAvailable)
    {
        // Setup: admin creates an unlocked dashboard
        _auth.LoginViaApi("default");
        _dashboard.CreateDashboardWithUniqueName();
        _auth.Logout();

        // Act: target user navigates to the unlocked dashboard
        _auth.LoginAs(login);
        _dashboard.NavigateToCreatedDashboard();

        Assert.That(_dashboard.IsLockAvailable(), Is.EqualTo(expectedAvailable),
            $"Lock button availability for role '{login}' should be {expectedAvailable}");
    }
}


using Allure.NUnit.Attributes;
using Business.Data;
using Business.Steps;
using NUnit.Framework;
using Core.Base;

namespace UITests.Tests.Dashboard;

/// <summary>
/// Feature: Dashboard Lock
/// Verifies that Lock/Unlock buttons are available only to privileged roles.
///
/// PROJECT_MANAGER role → can lock/unlock.
/// MEMBER, OPERATOR, CUSTOMER → cannot.
/// Permission is derived from each user's ProjectsAndRoles in the CSV.
/// </summary>
[TestFixture]
[Category("dashboard_lock")]
[AllureFeature("Dashboard")]
[AllureSuite("Lock / Unlock")]
public class DashboardLockTests : BaseTest
{
    private AuthSteps      _auth      = null!;
    private DashboardSteps _dashboard = null!;

    [SetUp]
    public void InitSteps()
    {
        _auth      = new AuthSteps();
        _dashboard = new DashboardSteps();
    }

    [TearDown]
    public void DeleteCreatedDashboard()
    {
        if (!_dashboard.HasCreatedDashboard) return;
        try
        {
            _auth.LoginAs("default");
            _dashboard.NavigateToCreatedDashboard();
            if (_dashboard.IsUnlockAvailable())
                _dashboard.UnlockDashboard();
            _dashboard.DeleteDashboard();
        }
        catch { /* cleanup best-effort */ }
    }

    [TestCaseSource(typeof(TestDataProvider), nameof(TestDataProvider.DashboardManagePermissions))]
    public void UnlockButton_Availability_ReflectsUserPermissions(string login, bool expectedAvailable)
    {
        // Setup: admin creates and locks the dashboard
        _auth.LoginAs("default");
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
        _auth.LoginAs("default");
        _dashboard.CreateDashboardWithUniqueName();
        _auth.Logout();

        // Act: target user navigates to the unlocked dashboard
        _auth.LoginAs(login);
        _dashboard.NavigateToCreatedDashboard();

        Assert.That(_dashboard.IsLockAvailable(), Is.EqualTo(expectedAvailable),
            $"Lock button availability for role '{login}' should be {expectedAvailable}");
    }
}

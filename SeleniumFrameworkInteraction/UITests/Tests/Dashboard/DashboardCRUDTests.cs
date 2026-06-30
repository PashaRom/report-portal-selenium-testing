using Allure.NUnit.Attributes;
using Business.Data;
using Core.Enum;
using UITests.Hooks;

namespace UITests.Tests.Dashboard;

/// <summary>
/// Feature: Dashboard CRUD
/// Scenario Outline: User creates a dashboard with a widget then deletes it
/// </summary>
[Category("dashboard_crud")]
[AllureFeature("Dashboard")]
[AllureSuite("CRUD")]
public class DashboardCRUDTests : DashboardTestBase
{
    public DashboardCRUDTests(BrowserType browser) : base(browser) { }

    [TestCaseSource(typeof(TestDataProvider), nameof(TestDataProvider.DashboardCrudCases))]
    [Description("User creates a named dashboard with a widget, then deletes the dashboard")]
    public void CreateDashboardWithWidget_ThenDelete(string login, string dashboardName, string widgetName)
    {
        _auth.LoginViaApi(login);
        _dashboard.CreateDashboardWithName(dashboardName);
        _dashboard.AddWidget("Launch statistics chart", widgetName);

        Assert.That(_dashboard.IsWidgetVisible(widgetName), Is.True,
            $"Widget '{widgetName}' should be visible after adding");

        _dashboard.DeleteDashboard();

        Assert.That(_dashboard.IsDashboardInList(dashboardName), Is.False,
            $"Dashboard '{dashboardName}' should no longer appear in the list after deletion");
    }
}

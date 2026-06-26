using Allure.NUnit.Attributes;
using Business.Data;
using Core.Enum;
using UITests.Hooks;

namespace UITests.Tests.Dashboard;

/// <summary>
/// Feature: Dashboard Widget Persistence
/// Scenario Outline: Dashboard widget persists after user re-logs in
/// </summary>
[Category("dashboard_create")]
[AllureFeature("Dashboard")]
[AllureSuite("Widget Persistence")]
public class DashboardCreateWithWidgetTests : DashboardTestBase
{
    public DashboardCreateWithWidgetTests(BrowserType browser) : base(browser) { }

    [TestCaseSource(typeof(TestDataProvider), nameof(TestDataProvider.LoginAliases))]
    [Description("Widget persists on the dashboard after the user logs out and back in")]
    public void Widget_PersistsAfterReLogin(string login)
    {
        const string widgetName = "Launch Stats Chart";

        _auth.LoginViaApi(login);
        _dashboard.CreateDashboardWithUniqueName();
        _dashboard.AddWidget("Launch statistics chart", widgetName);

        Assert.That(_dashboard.IsWidgetVisible(widgetName), Is.True,
            $"Widget '{widgetName}' should be visible after adding");

        _auth.Logout();
        _auth.LoginAs(login);
        _dashboard.NavigateToCreatedDashboard();

        Assert.That(_dashboard.IsWidgetVisible(widgetName), Is.True,
            $"Widget '{widgetName}' should persist after re-login");
    }
}

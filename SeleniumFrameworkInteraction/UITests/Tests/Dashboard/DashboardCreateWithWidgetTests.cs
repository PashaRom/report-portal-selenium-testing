using Allure.NUnit.Attributes;
using Business.Data;
using Business.Steps;
using NUnit.Framework;
using Core.Base;

namespace UITests.Tests.Dashboard;

/// <summary>
/// Feature: Dashboard Widget Persistence
/// Scenario Outline: Dashboard widget persists after user re-logs in
/// </summary>
[TestFixture]
[Category("dashboard_create")]
[AllureFeature("Dashboard")]
[AllureSuite("Widget Persistence")]
public class DashboardCreateWithWidgetTests : BaseTest
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
            _dashboard.DeleteDashboard();
        }
        catch { /* cleanup best-effort */ }
    }

    [TestCaseSource(typeof(TestDataProvider), nameof(TestDataProvider.LoginAliases))]
    [Description("Widget persists on the dashboard after the user logs out and back in")]
    public void Widget_PersistsAfterReLogin(string login)
    {
        const string widgetName = "Launch Stats Chart";

        _auth.LoginAs(login);
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

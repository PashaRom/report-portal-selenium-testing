using Allure.NUnit.Attributes;
using Business.Data;
using Business.Steps;
using NUnit.Framework;
using Core.Base;

namespace UITests.Tests.Dashboard;

/// <summary>
/// Feature: Dashboard CRUD
/// Scenario Outline: User creates a dashboard with a widget then deletes it
/// </summary>
[TestFixture]
[Category("dashboard_crud")]
[AllureFeature("Dashboard")]
[AllureSuite("CRUD")]
public class DashboardCRUDTests : BaseTest
{
    private AuthSteps      _auth      = null!;
    private DashboardSteps _dashboard = null!;

    [SetUp]
    public void InitSteps()
    {
        _auth      = new AuthSteps();
        _dashboard = new DashboardSteps();
    }

    [TestCaseSource(typeof(TestDataProvider), nameof(TestDataProvider.DashboardCrudCases))]
    [Description("User creates a named dashboard with a widget, then deletes the dashboard")]
    public void CreateDashboardWithWidget_ThenDelete(string login, string dashboardName, string widgetName)
    {
        _auth.LoginAs(login);
        _dashboard.CreateDashboardWithName(dashboardName);
        _dashboard.AddWidget("Launch statistics chart", widgetName);

        Assert.That(_dashboard.IsWidgetVisible(widgetName), Is.True,
            $"Widget '{widgetName}' should be visible after adding");

        _dashboard.DeleteDashboard();

        Assert.That(_dashboard.IsDashboardInList(dashboardName), Is.False,
            $"Dashboard '{dashboardName}' should no longer appear in the list after deletion");
    }
}

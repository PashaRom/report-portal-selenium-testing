using Allure.NUnit.Attributes;
using Business.Data;
using Business.Steps;
using Microsoft.Extensions.Logging;
using Core.Base;
using Core.DI;

namespace UITests.Tests.Dashboard;

/// <summary>
/// Feature: Dashboard All Widgets
/// Background: login as "default", create dashboard with a unique name
/// </summary>
[TestFixture]
[Category("dashboard_all_widgets")]
[AllureFeature("Dashboard")]
[AllureSuite("All Widgets")]
public class DashboardAllWidgetsTests : BaseTest
{
    private AuthSteps _auth = null!;
    private DashboardSteps _dashboard = null!;

    [SetUp]
    public void CreateDashboard()
    {
        _auth = ServiceLocator.GetService<AuthSteps>();
        _dashboard = ServiceLocator.GetService<DashboardSteps>();

        _auth.LoginAs("default");
        _dashboard.CreateDashboardWithUniqueName();
    }

    [TearDown]
    public void DeleteCreatedDashboard()
    {
        if (!_dashboard.HasCreatedDashboard)
        {
            Logger.LogWarning("No dashboard was created in this test, skipping cleanup");
            return;
        }
        try
        {
            _dashboard.NavigateToCreatedDashboard();
            _dashboard.DeleteDashboard();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Dashboard cleanup failed, skipping");
        }
    }

    [Test]
    [Description("Default user creates a dashboard with all available widgets and they persist after re-login")]
    public void AllWidgets_AreVisibleAndPersistAfterReLogin()
    {
        var allWidgets = WidgetTypesProvider.All;

        foreach (var widgetType in allWidgets)
            _dashboard.AddWidget(widgetType, widgetType);

        var visibleAfterAdd = _dashboard.CollectVisibleWidgetNames(allWidgets);
        Assert.That(visibleAfterAdd, Is.EquivalentTo(allWidgets),
            "All widgets should be visible immediately after adding");

        _auth.Logout();
        _auth.LoginAs("default");
        _dashboard.NavigateToCreatedDashboard();

        var visibleAfterReLogin = _dashboard.CollectVisibleWidgetNames(allWidgets);
        Assert.That(visibleAfterReLogin, Is.EquivalentTo(allWidgets),
            "All widgets should persist after re-login");
    }
}

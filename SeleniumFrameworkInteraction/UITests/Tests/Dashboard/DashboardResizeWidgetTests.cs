using Allure.NUnit.Attributes;
using Business.Data;
using Core.Enum;
using UITests.Hooks;

namespace UITests.Tests.Dashboard
{
    [Category("dashboard_resize")]
    [AllureFeature("Dashboard")]
    [AllureSuite("Resize")]

    public class DashboardResizeWidgetTests : DashboardTestBase
    {
        public DashboardResizeWidgetTests(BrowserType browser) : base(browser) { }

        [Test]
        [Description("Default user creates a dashboard with 5 available widgets and resize ones")]
        public void ResizeWidget_ResizeWidgetAndCheckSize()
        {
            var allWidgets = WidgetTypesProvider.All.ToList();
            var widgetOrder = 1;
            var resizeValue = 80;

            _auth.LoginViaApi("default");
            _dashboard.CreateDashboardWithUniqueName();
            _dashboard.AddWidgets(allWidgets.Take(6).ToList());

            var originalWidgetSize = _dashboard.GetWidgetByName(allWidgets[widgetOrder])?.Size;
            _dashboard.ResizeWidget(allWidgets[widgetOrder], resizeValue, resizeValue);
            var resizedWidgetSize = _dashboard.GetWidgetByName(allWidgets[widgetOrder])?.Size;

            Assert.Multiple(() =>
            {
                Assert.That(resizedWidgetSize?.Width, Is.GreaterThan(originalWidgetSize?.Width), $"Width of widget '{allWidgets[widgetOrder]}' has not be changed");
                Assert.That(resizedWidgetSize?.Height, Is.GreaterThan(originalWidgetSize?.Height), $"Height of widget '{allWidgets[widgetOrder]}' has not be changed");
            });

        }
    }


}

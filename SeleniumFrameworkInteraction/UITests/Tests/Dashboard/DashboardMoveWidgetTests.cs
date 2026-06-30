using Allure.NUnit.Attributes;
using Business.Data;
using Core.Enum;
using UITests.Hooks;

namespace UITests.Tests.Dashboard
{
    [Category("dashboard_movement_widget")]
    [AllureFeature("Dashboard")]
    [AllureSuite("Movement widget")]

    public class DashboardMoveWidgetTests : DashboardTestBase
    {
        private readonly List<string> _widgets;
        public DashboardMoveWidgetTests(BrowserType browser) : base(browser)
        {
            _widgets = WidgetTypesProvider.GetAllWidgets().Take(3).ToList();
        }

        [SetUp]
        public void SetUpPrecondotopn()
        {
            _auth.LoginViaApi("default");
            _dashboard.CreateDashboardWithUniqueName();
            _dashboard.AddWidgets(_widgets);
        }

        [Test]
        [Description("Move widget and check new location")]
        public void MoveWidget_MoveVidgetAndCheckNewLocation()
        {
            var elementNumber = 1;
            var originalWidgetLocation = _dashboard.GetWidgetByName(_widgets.ElementAt(elementNumber))?.Location;
            var originalWidgetSize = _dashboard.GetWidgetByName(_widgets.ElementAt(elementNumber))?.Size;
            _dashboard.MoveWidget(_widgets.ElementAt(elementNumber), originalWidgetSize?.Height, Movement.Top);
            _dashboard.MoveWidget(_widgets.ElementAt(elementNumber), originalWidgetSize?.Width, Movement.Right);
            var originalWidgetLocationAfterMove = _dashboard.GetWidgetByName(_widgets.ElementAt(elementNumber))?.Location;

            Assert.Multiple(() =>
            {
                Assert.That(originalWidgetLocationAfterMove?.X, Is.GreaterThan(originalWidgetLocation?.X), "Widget did not move right as expected.");
                Assert.That(originalWidgetLocationAfterMove?.Y, Is.LessThan(originalWidgetLocation?.Y), "Widget did not move up as expected.");
            });
        }
    }
}

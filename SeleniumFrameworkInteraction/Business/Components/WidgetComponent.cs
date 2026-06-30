using Business.Locators;
using Core.Base;
using Core.Elements;
using Core.Enum;
using Core.Helpers;
using OpenQA.Selenium;
using System.Drawing;

namespace Business.Components
{
    public class WidgetComponent : BaseComponent
    {
        public WidgetComponent() : base("Widget", CommonLocators.Widget) { }
        public WidgetComponent(IWebElement element) : base(element) { }

        private Label Title => new(By.CssSelector("div[class*=\"widgetHeader\"][class*=\"type\"] span"), "Header", Root);
        private Button ResizeHandle => new(By.CssSelector(".react-resizable-handle"), "Resize Handle", Root);
        private Label HeaderPanel => new(By.CssSelector("div[class*='widget-header'] [class*=widget-header]"), "Widget header panel", Root);
        public string TitleText => Title.Value;

        public Point GetRightBottomCoordinate()
        {
            var location = Root.Location;
            var size = Root.Size;
            return new Point(
                location.X + size.Width,
                location.Y + size.Height
            );
        }

        public void Resize(int xOffset, int yOffset) =>
            ActionHelper.Resize(ResizeHandle.Element, Name, xOffset, yOffset);

        public void DragAndDropTo(int? offset, Movement movement) =>
            ActionHelper.DragAndDropByOffset(HeaderPanel.Element, TitleText, offset, movement);
    }
}

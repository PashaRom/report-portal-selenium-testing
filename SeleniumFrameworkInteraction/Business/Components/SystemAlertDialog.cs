using Core.Base;
using Core.Elements;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Business.Components
{
    public class SystemAlertDialog : BaseComponent
    {
        public SystemAlertDialog() : base(
        "System Alert Dialog",
        By.CssSelector("div[data-automation-id=\"notificationsContainer\"] div.notification-transition-enter-active"))
        { }

        private Button CloseButtom => new(By.XPath("//button[contains(@aria-label,\"close\")]"), "Close System Alert Dialog Button", Root);

        public void Close()
        {
            Logger.LogInformation("[{Component}] Clicking Close button", Name);
            CloseButtom.Click();
        }

        public bool IsDisplayed
        {
            get
            {
                try
                {
                    return Root.Displayed;
                }
                catch (NoSuchElementException ex)
                {
                    Logger.LogInformation(ex, "[{Component}] System Alert Dialog is not displayed", Name);
                    return false;
                }
            }
        }
    }
}

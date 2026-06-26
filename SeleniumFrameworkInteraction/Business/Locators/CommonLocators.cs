using OpenQA.Selenium;

namespace Business.Locators
{
    public sealed class CommonLocators
    {
        public static By Widget => By.CssSelector("div[class*='widgetsGrid']");
    }
}

using OpenQA.Selenium;

namespace Business.Locators
{
    public static class CommonLocators
    {
        public static By Widget => By.CssSelector("div[class*='widgetsGrid']");
    }
}

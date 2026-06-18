using Core.Drivers;
using Core.Logging;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Core.Base;

public abstract class BasePage : BaseApplication
{
    protected BasePage(int waitTimeoutSeconds = 20)
        : base(waitTimeoutSeconds)
    {
    }

    public string CurrentUrl => Driver.Url;
    public string Title => Driver.Title;

    public void NavigateTo(string url)
        => NavigateAndWaitForReady(url);

    protected void NavigateAndWaitForReady(string url)
    {
        Logger.LogInformation("Navigating to {Url}", url);
        Driver.Navigate().GoToUrl(url);
        Wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState")?.Equals("complete") == true);
    }

    protected override IWebElement FindElement(By locator)
        => Wait.Until(d => d.FindElement(locator));

    protected override IReadOnlyCollection<IWebElement> FindElements(By locator)
        => Driver.FindElements(locator);

    protected override bool IsElementDisplayed(By locator)
    {
        try { return Driver.FindElement(locator).Displayed; }
        catch (NoSuchElementException) { return false; }
    }

    protected IWebElement WaitUntilClickable(By locator)
        => Wait.Until(d =>
        {
            try
            {
                var el = d.FindElement(locator);
                return el.Displayed && el.Enabled ? el : null;
            }
            catch (StaleElementReferenceException) { return null; }
        });
}

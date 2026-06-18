using Core.Drivers;
using Core.Logging;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Core.Base;

public abstract class BaseApplication
{
    protected readonly IWebDriver Driver;
    protected readonly ILogger Logger;
    protected readonly WebDriverWait Wait;

    protected BaseApplication(int waitTimeoutSeconds)
    {
        Driver = DriverManager.Current;
        Logger = TestLoggerFactory.CreateLogger(GetType().Name);
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTimeoutSeconds));
    }

    protected abstract IWebElement FindElement(By locator);

    protected abstract IReadOnlyCollection<IWebElement> FindElements(By locator);

    protected abstract bool IsElementDisplayed(By locator);
}
using Allure.Net.Commons;
using Allure.NUnit;
using Core.DI;
using Core.Drivers;
using Core.Enum;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;

namespace Core.Base;

[TestFixtureSource(typeof(BrowserDataSource), nameof(BrowserDataSource.Browsers))]
[AllureNUnit]
public abstract class BaseTest
{
    private readonly BrowserType _browser;

    protected ILogger Logger { get; }

    protected BaseTest(BrowserType browser)
    {
        _browser = browser;
        Logger = ServiceLocator.GetService<ILoggerFactory>().CreateLogger(GetType().Name);
    }

    [SetUp]
    public void InitDriver()
    {
        var driver = ServiceLocator.GetService<IWebDriverFactory>().Create(_browser);
        ServiceLocator.GetService<IDriverManager>().Set(driver);
    }

    [TearDown]
    public void QuitDriver()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
        {
            TryAttachScreenshot();
        }

        ServiceLocator.GetService<IDriverManager>().Quit();
    }

    private void TryAttachScreenshot()
    {
        try
        {
            var driver = ServiceLocator.GetService<IDriverManager>().Current;
            if (driver is ITakesScreenshot screenshotDriver)
            {
                var bytes = screenshotDriver.GetScreenshot().AsByteArray;
                AllureApi.AddAttachment("Screenshot on Failure", "image/png", bytes, ".png");
                Logger.LogInformation("[BaseTest] Screenshot attached to Allure report");
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[BaseTest] Could not take screenshot on test failure");
        }
    }
}

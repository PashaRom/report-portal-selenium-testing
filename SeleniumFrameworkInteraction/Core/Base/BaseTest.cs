using Allure.NUnit;
using Core.DI;
using Core.Drivers;
using Core.Enum;
using Core.Utils;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

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
            ScreenshotUtil.TryAttachScreenshotToAllReports();
            Logger.LogInformation("[BaseTest] Screenshot attached to Allure report and Report Portal");
        }

        ServiceLocator.GetService<IDriverManager>().Quit();
    }
}

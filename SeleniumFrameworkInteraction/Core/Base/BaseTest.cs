using Allure.NUnit;
using Core.DI;
using Core.Drivers;
using Core.Enum;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

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
        ServiceLocator.GetService<IDriverManager>().Quit();
    }
}

using Allure.NUnit;
using Core.DI;
using Core.Drivers;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Core.Base;

[TestFixture]
[AllureNUnit]
public abstract class BaseTest
{
    protected ILogger Logger { get; }

    protected BaseTest()
    {
        Logger = ServiceLocator.GetService<ILoggerFactory>().CreateLogger(GetType().Name);
    }

    [SetUp]
    public void InitDriver()
    {
        var driver = ServiceLocator.GetService<IWebDriverFactory>().Create();
        ServiceLocator.GetService<IDriverManager>().Set(driver);
    }

    [TearDown]
    public void QuitDriver()
    {
        ServiceLocator.GetService<IDriverManager>().Quit();
    }
}

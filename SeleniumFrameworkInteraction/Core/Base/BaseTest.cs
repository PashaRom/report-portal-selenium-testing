using Allure.NUnit;
using Allure.NUnit.Attributes;
using Core.Drivers;
using NUnit.Framework;

namespace Core.Base;

[TestFixture]
[AllureNUnit]
public abstract class BaseTest
{
    [SetUp]
    public void InitDriver()
    {
        DriverManager.Set(WebDriverFactory.Create());
    }

    [TearDown]
    public void QuitDriver()
    {
        DriverManager.Quit();
    }
}

using Core.Enum;
using OpenQA.Selenium;

namespace Core.Drivers;

public interface IWebDriverFactory
{
    IWebDriver Create(BrowserType browser);
}

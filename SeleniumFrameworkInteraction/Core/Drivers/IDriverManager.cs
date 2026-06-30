using OpenQA.Selenium;

namespace Core.Drivers;

public interface IDriverManager
{
    IWebDriver Current { get; }
    bool IsInitialized { get; }
    void Set(IWebDriver driver);
    void Quit();
}

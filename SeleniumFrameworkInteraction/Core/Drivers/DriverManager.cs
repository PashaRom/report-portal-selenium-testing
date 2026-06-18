using OpenQA.Selenium;

namespace Core.Drivers;

public static class DriverManager
{
    private static readonly ThreadLocal<IWebDriver?> _driver = new();

    public static IWebDriver Current =>
        _driver.Value ?? throw new InvalidOperationException(
            "WebDriver is not initialized for the current thread. Call DriverManager.Set() before accessing Current.");

    public static bool IsInitialized => _driver.Value != null;

    public static void Set(IWebDriver driver) => _driver.Value = driver;

    public static void Quit()
    {
        if (_driver.Value == null)
            return;

        _driver.Value.Quit();
        _driver.Value = null;
    }
}

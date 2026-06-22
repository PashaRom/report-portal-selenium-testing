using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Core.Drivers;

public class DriverManager : IDriverManager
{
    private readonly ThreadLocal<IWebDriver?> _driver = new();
    private readonly ILogger<DriverManager> _logger;

    public DriverManager(ILogger<DriverManager> logger)
    {
        _logger = logger;
    }

    public IWebDriver Current =>
        _driver.Value ?? throw new InvalidOperationException(
            "WebDriver is not initialized for the current thread. Call IDriverManager.Set() before accessing Current.");

    public bool IsInitialized => _driver.Value != null;

    public void Set(IWebDriver driver) => _driver.Value = driver;

    public void Quit()
    {
        if (_driver.Value == null)
        {
            _logger.LogDebug("WebDriver is not initialized for the current thread, skipping Quit.");
            return;
        }

        _driver.Value.Quit();
        _driver.Value = null;
    }
}

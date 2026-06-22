using Core.DI;
using OpenQA.Selenium;

namespace Core.Drivers;

/// <summary>
/// Ambient context that exposes the current thread's WebDriver.
/// Thread-safe: the backing IDriverManager stores drivers in a ThreadLocal,
/// so each parallel test thread receives its own isolated IWebDriver instance.
/// </summary>
public static class DriverContext
{
    private static readonly IDriverManager _manager =
        ServiceLocator.GetService<IDriverManager>();

    public static IWebDriver Current => _manager.Current;
}

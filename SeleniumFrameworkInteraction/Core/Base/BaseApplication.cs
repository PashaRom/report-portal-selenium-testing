using Core.DI;
using Core.Configuration;
using Core.Drivers;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Core.Base;

/// <summary>
/// Base class for all Page Objects and Components.
/// Provides shared access to WebDriver, Logger, and configuration via DI.
/// </summary>
public abstract class BaseApplication
{
    protected int ExplicitWaitTimeoutSeconds { get; } =
        ServiceLocator.GetService<IAppConfiguration>().ExplicitWaitTimeoutSeconds;

    protected IWebDriver Driver => DriverContext.Current;

    protected IAppConfiguration Configuration => ServiceLocator.GetService<IAppConfiguration>();

    protected ILogger Logger { get; }

    protected BaseApplication()
    {
        Logger = ServiceLocator.GetService<ILoggerFactory>().CreateLogger(GetType().Name);
    }
}

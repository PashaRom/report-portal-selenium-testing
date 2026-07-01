using Core.DI;
using Core.Drivers;
using Core.Elements;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Core.Helpers;

public static class WaitHelper
{
    private static readonly TimeSpan DefaultTimeout    = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan DefaultElementTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan DefaultPolling    = TimeSpan.FromMilliseconds(100);

    private static ILogger Logger =>
        ServiceLocator.GetService<ILoggerFactory>().CreateLogger(nameof(WaitHelper));

    private static readonly Type[] DefaultIgnoredExceptions =
    [
        typeof(NoSuchElementException),
        typeof(StaleElementReferenceException),
        typeof(ElementNotInteractableException),
        typeof(TimeoutException)
    ];

    /// <summary>
    /// Waits until the condition returns a non-null/non-false value, then returns it.
    /// Uses WebDriverWait with common transient exceptions ignored.
    /// </summary>
    /// <param name="condition">Condition to evaluate against the current driver.</param>
    /// <param name="timeout">Wait timeout. Default is 10 seconds.</param>
    /// <param name="polling">Polling interval. Default is 100 ms.</param>
    /// <param name="ignoredExceptions">Exception types to ignore. Defaults to common transient exceptions.</param>
    /// <param name="elementName">Optional element name for logging.</param>
    /// <param name="driver">WebDriver instance. Defaults to the current driver.</param>
    public static T Until<T>(
        Func<IWebDriver, T> condition,
        TimeSpan? timeout = null,
        TimeSpan? polling = null,
        IEnumerable<Type>? ignoredExceptions = null,
        string? elementName = null,
        IWebDriver? driver = null)
    {
        var t = timeout ?? DefaultTimeout;
        var p = polling ?? DefaultPolling;
        LogWaitStart(nameof(Until), t, p, ignoredExceptions, elementName);
        try
        {
            var result = CreateWait(t, p, ignoredExceptions, driver).Until(condition);
            LogWaitSuccess(nameof(Until), elementName);
            return result;
        }
        catch (Exception ex)
        {
            LogWaitFailure(nameof(Until), elementName, ex);
            throw;
        }
    }

    /// <summary>
    /// Waits until the given element is displayed and enabled, then returns it.
    /// Handles StaleElementReferenceException by re-finding via the element's SearchContext + Locator.
    /// The element's Name is used automatically for logging.
    /// </summary>
    /// <param name="element">Wrapper element to wait for.</param>
    /// <param name="timeout">Wait timeout. Default is 5 seconds.</param>
    /// <param name="polling">Polling interval. Default is 100 ms.</param>
    /// <param name="ignoredExceptions">Exception types to ignore. Defaults to common transient exceptions.</param>
    /// <param name="driver">WebDriver instance. Defaults to the current driver.</param>
    public static IWebElement DefaultWait(
        IWrapperElement element,
        TimeSpan? timeout = null,
        TimeSpan? polling = null,
        IEnumerable<Type>? ignoredExceptions = null,
        IWebDriver? driver = null)
    {
        var t = timeout ?? DefaultElementTimeout;
        var p = polling ?? DefaultPolling;
        Logger.LogDebug(
            "[WaitHelper.DefaultWait] Waiting for element '{Name}' | timeout={Timeout}, polling={Polling}ms",
            element.Name, t, p);

        try
        {
            var result = Until<IWebElement?>(d =>
            {
                try
                {
                    if (element.Locator is null)
                    {
                        var preFound = element.Element;
                        return preFound.Displayed && preFound.Enabled ? preFound : null;
                    }

                    var ctx = element.SearchContext ?? (ISearchContext)d;
                    var el = ctx.FindElement(element.Locator);
                    return el.Displayed && el.Enabled ? el : null;
                }
                catch (StaleElementReferenceException ex)
                {
                    Logger.LogDebug(ex, "[WaitHelper.DefaultWait] StaleElementReferenceException for '{Name}', retrying...", element.Name);
                    return null;
                }
            }, t, p, ignoredExceptions, elementName: element.Name, driver)!;

            Logger.LogDebug("[WaitHelper.DefaultWait] Element '{Name}' is ready", element.Name);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex,"[WaitHelper.DefaultWait] Element '{Name}' did not become ready", element.Name);
        }
    }

    private static WebDriverWait CreateWait(
        TimeSpan timeout,
        TimeSpan polling,
        IEnumerable<Type>? ignoredExceptions = null,
        IWebDriver? driver = null)
    {
        var wait = new WebDriverWait(driver ?? ServiceLocator.GetService<IDriverManager>().Current, timeout)
        {
            PollingInterval = polling
        };

        wait.IgnoreExceptionTypes(ignoredExceptions?.ToArray() ?? DefaultIgnoredExceptions);
        return wait;
    }

    private static void LogWaitStart(string method, TimeSpan timeout, TimeSpan polling, IEnumerable<Type>? ignoredExceptions, string? elementName)
    {
        var exceptions = ignoredExceptions is not null
            ? string.Join(", ", ignoredExceptions.Select(t => t.Name))
            : "default";

        if (elementName is not null)
            Logger.LogDebug("[WaitHelper.{Method}] Element='{Name}' | timeout={Timeout}, polling={Polling}ms, ignored=[{Exceptions}]",
                method, elementName, timeout, polling.TotalMilliseconds, exceptions);
        else
            Logger.LogDebug("[WaitHelper.{Method}] timeout={Timeout}, polling={Polling}ms, ignored=[{Exceptions}]",
                method, timeout, polling.TotalMilliseconds, exceptions);
    }

    private static void LogWaitSuccess(string method, string? elementName)
    {
        if (elementName is not null)
            Logger.LogDebug("[WaitHelper.{Method}] Element='{Name}': condition met", method, elementName);
        else
            Logger.LogDebug("[WaitHelper.{Method}] Condition met", method);
    }

    private static void LogWaitFailure(string method, string? elementName, Exception ex)
    {
        if (elementName is not null)
            Logger.LogWarning("[WaitHelper.{Method}] Element='{Name}': wait failed — {Error}",
                method, elementName, ex.Message);
        else
            Logger.LogWarning("[WaitHelper.{Method}] Wait failed — {Error}", method, ex.Message);
    }
}

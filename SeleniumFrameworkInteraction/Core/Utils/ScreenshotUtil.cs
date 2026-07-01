using Allure.Net.Commons;
using Core.DI;
using Core.Drivers;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using ReportPortal.Shared;

namespace Core.Utils;

public static class ScreenshotUtil
{
    // ThreadStatic is safe for parallel test execution — each thread (test) has its own slot
    [ThreadStatic]
    private static IWebElement? _lastTrackedElement;

    private static ILogger Logger =>
        ServiceLocator.GetService<ILoggerFactory>().CreateLogger(nameof(ScreenshotUtil));

    /// <summary>
    /// Records the most recently resolved element for the current thread.
    /// Called from <see cref="Core.Elements.WrapperElement.Element"/> on every successful resolution.
    /// </summary>
    public static void TrackElement(IWebElement element)
    {
        _lastTrackedElement = element;
    }

    /// <summary>
    /// Takes a full-page screenshot, highlights the last tracked element with a red border
    /// (if still available in the DOM), and attaches the image to the Allure report.
    /// </summary>
    public static void TryAttachScreenshot()
    {
        try
        {
            var driver = ServiceLocator.GetService<IDriverManager>().Current;

            if (driver is not ITakesScreenshot screenshotDriver)
            {
                return;
            }

            TryHighlightLastElement(driver);
            var bytes = screenshotDriver.GetScreenshot().AsByteArray;
            TryRestoreLastElement(driver);

            AllureApi.AddAttachment("Screenshot on Failure", "image/png", bytes, ".png");
            Logger.LogInformation("[ScreenshotUtil] Screenshot attached to Allure report");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[ScreenshotUtil] Could not take screenshot on test failure");
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static void TryHighlightLastElement(IWebDriver driver)
    {
        var el = _lastTrackedElement;
        if (el is null)
        {
            return;
        }

        try
        {
            ((IJavaScriptExecutor)driver).ExecuteScript(
                "arguments[0].style.outline = '4px solid red';" +
                "arguments[0].style.outlineOffset = '2px';",
                el);
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "[ScreenshotUtil] Could not highlight element");
        }
    }

    private static void TryRestoreLastElement(IWebDriver driver)
    {
        var el = _lastTrackedElement;
        if (el is null)
        {
            return;
        }

        try
        {
            ((IJavaScriptExecutor)driver).ExecuteScript(
                "arguments[0].style.outline = '';" +
                "arguments[0].style.outlineOffset = '';",
                el);
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "[ScreenshotUtil] Could not restore element style");
        }
    }

    /// <summary>
    /// Takes a full-page screenshot, highlights the last tracked element with a red border
    /// (if still available in the DOM), and attaches the image to both Allure and ReportPortal reports.
    /// </summary>
    public static void TryAttachScreenshotToAllReports()
    {
        try
        {
            var driver = ServiceLocator.GetService<IDriverManager>().Current;

            if (driver is not ITakesScreenshot screenshotDriver)
            {
                return;
            }

            TryHighlightLastElement(driver);
            var bytes = screenshotDriver.GetScreenshot().AsByteArray;
            TryRestoreLastElement(driver);

            // Allure
            AllureApi.AddAttachment("Screenshot on Failure", "image/png", bytes, ".png");
            Logger.LogInformation("[ScreenshotUtil] Screenshot attached to Allure report");

            // ReportPortal
            if (Context.Current != null)
            {
                Context.Current.Log.Error("Screenshot on Failure", "image/png", bytes);
                Logger.LogInformation("[ScreenshotUtil] Screenshot attached to ReportPortal");
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[ScreenshotUtil] Could not take screenshot on test failure");
        }
    }
}


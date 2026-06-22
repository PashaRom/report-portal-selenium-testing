using Core.DI;
using Core.Drivers;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace Core.Helpers;

public static class ActionHelper
{
    private static ILogger Logger =>
        ServiceLocator.GetService<ILoggerFactory>().CreateLogger(nameof(ActionHelper));

    private static IJavaScriptExecutor Js =>
        (IJavaScriptExecutor)DriverContext.Current;

    // ── Mouse Actions ────────────────────────────────────────────────────────

    /// <summary>
    /// Moves the cursor to the element and clicks it via Selenium Actions.
    /// Use when a standard click is intercepted or the element requires hover to become interactive.
    /// </summary>
    public static void MoveToElementAndClick(IWebElement element, string elementName)
    {
        Logger.LogInformation("[ActionHelper] {Name}: moving to element and clicking via Actions", elementName);

        new Actions(DriverContext.Current)
            .MoveToElement(element)
            .Click()
            .Perform();

        Logger.LogInformation("[ActionHelper] {Name}: clicked successfully via Actions", elementName);
    }

    /// <summary>
    /// Drags the source element and drops it onto the target element via Selenium Actions.
    /// </summary>
    public static void DragAndDrop(IWebElement source, string sourceName, IWebElement target, string targetName)
    {
        Logger.LogInformation("[ActionHelper] DragAndDrop: '{Source}' → '{Target}'", sourceName, targetName);

        new Actions(DriverContext.Current)
            .DragAndDrop(source, target)
            .Perform();

        Logger.LogInformation("[ActionHelper] DragAndDrop: completed '{Source}' → '{Target}'", sourceName, targetName);
    }

    /// <summary>
    /// Drags the source element and drops it at the given pixel offset from its current position.
    /// </summary>
    public static void DragAndDropByOffset(IWebElement source, string sourceName, int offsetX, int offsetY)
    {
        Logger.LogInformation(
            "[ActionHelper] DragAndDropByOffset: '{Source}' by ({X}px, {Y}px)",
            sourceName, offsetX, offsetY);

        new Actions(DriverContext.Current)
            .DragAndDropToOffset(source, offsetX, offsetY)
            .Perform();

        Logger.LogInformation(
            "[ActionHelper] DragAndDropByOffset: completed '{Source}' by ({X}px, {Y}px)",
            sourceName, offsetX, offsetY);
    }

    // ── JavaScript Actions ───────────────────────────────────────────────────

    /// <summary>
    /// Clicks an element via JavaScript. Use when a standard or Actions-based click
    /// is blocked by overlapping elements or animation.
    /// </summary>
    public static void JsClick(IWebElement element, string elementName)
    {
        Logger.LogInformation("[ActionHelper] {Name}: clicking via JavaScript", elementName);
        Js.ExecuteScript("arguments[0].click();", element);
        Logger.LogInformation("[ActionHelper] {Name}: clicked successfully via JavaScript", elementName);
    }

    /// <summary>
    /// Clears localStorage and sessionStorage for the current origin.
    /// Call before login to ensure a clean browser state.
    /// </summary>
    public static void JsClearBrowserStorage()
    {
        Logger.LogInformation("[ActionHelper] Clearing localStorage and sessionStorage");
        Js.ExecuteScript("localStorage.clear(); sessionStorage.clear();");
    }

    /// <summary>
    /// Returns the best scrollable container element on the page (largest scrollable overflow area),
    /// or null if no scrollable container is found.
    /// </summary>
    public static IWebElement? JsFindScrollableContainer()
    {
        Logger.LogDebug("[ActionHelper] Searching for scrollable container");
        return Js.ExecuteScript(@"
            var best = null, bestDiff = 100;
            Array.from(document.querySelectorAll('*')).forEach(function(el) {
                var s = window.getComputedStyle(el);
                if (s.overflowY !== 'auto' && s.overflowY !== 'scroll') return;
                var diff = el.scrollHeight - el.clientHeight;
                if (diff > bestDiff) { bestDiff = diff; best = el; }
            });
            return best;
        ") as IWebElement;
    }

    /// <summary>
    /// Scrolls the given container element to the top (scrollTop = 0).
    /// </summary>
    public static void JsScrollToTop(IWebElement container)
    {
        Logger.LogDebug("[ActionHelper] Scrolling container to top");
        Js.ExecuteScript("arguments[0].scrollTop = 0;", container);
    }

    /// <summary>
    /// Returns the current scrollTop value of the given container element.
    /// </summary>
    public static long JsGetScrollTop(IWebElement container)
    {
        return Convert.ToInt64(Js.ExecuteScript("return arguments[0].scrollTop;", container));
    }

    /// <summary>
    /// Scrolls the given container element down by the specified number of pixels.
    /// </summary>
    public static void JsScrollBy(IWebElement container, int pixels)
    {
        Logger.LogDebug("[ActionHelper] Scrolling container by {Pixels}px", pixels);
        Js.ExecuteScript($"arguments[0].scrollTop += {pixels};", container);
    }
}

using Core.DI;
using Core.Enum;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace Core.Helpers;

public static class ActionHelper
{
    private static ILogger Logger =>
        ServiceLocator.GetService<ILoggerFactory>().CreateLogger(nameof(ActionHelper));

    private static IWebDriver CurrentDriver =>
        ServiceLocator.GetService<Core.Drivers.IDriverManager>().Current;

    private static IJavaScriptExecutor Js =>
        (IJavaScriptExecutor)CurrentDriver;

    // ── Mouse Actions ────────────────────────────────────────────────────────

    /// <summary>
    /// Moves the cursor to the element and clicks it via Selenium Actions.
    /// Use when a standard click is intercepted or the element requires hover to become interactive.
    /// </summary>
    public static void MoveToElementAndClick(IWebElement? element, string elementName)
    {
        Logger.LogInformation("[ActionHelper] {Name}: moving to element and clicking via Actions", elementName);
        ArgumentNullException.ThrowIfNull(element);
        new Actions(CurrentDriver)
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

        new Actions(CurrentDriver)
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

        new Actions(CurrentDriver)
            .DragAndDropToOffset(source, offsetX, offsetY)
            .Perform();

        Logger.LogInformation(
            "[ActionHelper] DragAndDropByOffset: completed '{Source}' by ({X}px, {Y}px)",
            sourceName, offsetX, offsetY);
    }

    /// <summary>
    /// Drags the source element and drops it at the given pixel offset in the specified direction (Movement).
    /// </summary>
    public static void DragAndDropByOffset(IWebElement source, string sourceName, int? offset, Movement movement)
    {
        int newOffset = offset ?? 0;
        var (offsetX, offsetY) = movement switch
        {
            Movement.Left => (-newOffset, 0),
            Movement.Right => (newOffset, 0),
            Movement.Top => (0, -newOffset),
            Movement.Bottom => (0, newOffset),
            _ => (0, 0)
        };

        Logger.LogInformation(
            "[ActionHelper] DragAndDropByOffset: '{Source}' by ({X}px, {Y}px)",
            sourceName, offsetX, offsetY);

        const int steps = 20;
        const int stepDelayMs = 30;

        int stepX = offsetX / steps;
        int stepY = offsetY / steps;

        var actions = new Actions(CurrentDriver);
        actions.MoveToElement(source)
               .ClickAndHold()
               .Pause(TimeSpan.FromMilliseconds(300));

        for (int i = 0; i < steps; i++)
        {
            actions.MoveByOffset(stepX, stepY)
                   .Pause(TimeSpan.FromMilliseconds(stepDelayMs));
        }

        actions.Release()
               .Pause(TimeSpan.FromMilliseconds(300))
               .Perform();

        Logger.LogInformation(
            "[ActionHelper] DragAndDropByOffset: completed '{Source}' by ({X}px, {Y}px)",
            sourceName, offsetX, offsetY);
    }

    /// <summary>
    /// Resize element at the given pixel offset from its current size.
    /// </summary>

    public static void Resize(IWebElement handle, string? sourceName, int offsetX, int offsetY)
    {
        var actions = new Actions(CurrentDriver);
        actions.MoveToElement(handle)
               .Pause(TimeSpan.FromMilliseconds(200))
               .ClickAndHold()
               .Pause(TimeSpan.FromMilliseconds(200));

        const int pixelsPerStep = 5;
        int steps = Math.Max(Math.Abs(offsetX), Math.Abs(offsetY)) / pixelsPerStep;
        steps = Math.Max(steps, 1);

        double stepX = (double)offsetX / steps;
        double stepY = (double)offsetY / steps;
        int accumulatedX = 0;
        int accumulatedY = 0;

        for (int i = 0; i < steps; i++)
        {
            int targetX = (int)Math.Round(stepX * (i + 1));
            int targetY = (int)Math.Round(stepY * (i + 1));
            int moveX = targetX - accumulatedX;
            int moveY = targetY - accumulatedY;
            accumulatedX += moveX;
            accumulatedY += moveY;

            actions.MoveByOffset(moveX, moveY)
                   .Pause(TimeSpan.FromMilliseconds(50));
        }

        actions.Release().Perform();
    }

    /// <summary>
    /// Scroll to element.
    /// </summary>

    public static void ScrollTo(IWebElement element, string sourceName)
    {
        Logger.LogInformation("[ActionHelper] Scroll: '{Source}'", sourceName);

        new Actions(CurrentDriver)
            .ScrollToElement(element)
            .Perform();

        Logger.LogInformation("[ActionHelper] Scroll: '{Source}'", sourceName);
    }

    /// <summary>
    /// Scrolls the page until the given element is at the top of the viewport (or as close as possible).
    /// </summary>

    public static void ScrollToElementTop(IWebElement? element, string? elementName)
    {
        var driver = CurrentDriver;
        var actions = new Actions(driver);
        ArgumentNullException.ThrowIfNull(element);

        // фиксируем фокус
        driver.FindElement(By.TagName("body")).Click();

        WaitHelper.Until(_ =>
        {
            int y = element.Location.Y;

            // ✅ достигли цели
            if (y >= 0 && y <= 5)
                return true;

            // ✅ адаптивный скролл
            if (y > 200)
                actions.SendKeys(Keys.PageDown).Perform();
            else if (y > 0)
                actions.SendKeys(Keys.ArrowDown).Perform();
            else
                actions.SendKeys(Keys.ArrowUp).Perform();

            return false;

        }, elementName: elementName);
    }

    // ── JavaScript Actions ───────────────────────────────────────────────────

    /// <summary>
    /// Clicks an element via JavaScript. Use when a standard or Actions-based click
    /// is blocked by overlapping elements or animation.
    /// </summary>
    public static void JsClick(IWebElement? element, string elementName)
    {
        Logger.LogInformation("[ActionHelper] {Name}: clicking via JavaScript", elementName);
        ArgumentNullException.ThrowIfNull(element);
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
    /// Sets a single localStorage item for the current origin.
    /// </summary>
    public static void JsSetLocalStorageItem(string key, string value)
    {
        Logger.LogInformation("[ActionHelper] Setting localStorage[{Key}]", key);
        Js.ExecuteScript("localStorage.setItem(arguments[0], arguments[1]);", key, value);
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

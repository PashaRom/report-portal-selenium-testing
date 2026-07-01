using Core.Helpers;
using Core.Structures;
using OpenQA.Selenium;

namespace Core.Elements;

/// <summary>
/// Represents a clickable button element.
/// Handles click actions with explicit wait for visibility and enabled state.
/// </summary>
public class Button : WrapperElement
{
    public Button(By locator, string name) : base(locator, name) { }
    public Button(By locator, string name, ISearchContext context) : base(locator, name, context) { }
    public Button(IWebElement element, string name) : base(element, name) { }

    /// <summary>
    /// Clicks the button after waiting for it to be clickable (displayed and enabled).
    /// </summary>
    public void Click()
    {
        LogAction($"Clicking on Button element {Name}");
        WaitHelper.DefaultWait(this, timeout: Timeouts.Sec5)?.Click();
        LogAction($"Clicked on Button element {Name} successfully");
    }

    /// <summary>
    /// Moves the cursor to the button via Actions, then clicks it.
    /// Use when a standard click is intercepted or the element requires hover to become interactive.
    /// </summary>
    public void ClickWithActions()
    {
        LogAction($"Clicking on Button element {Name} via Actions");
        var element = WaitHelper.DefaultWait(this, timeout: Timeouts.Sec5);
        ActionHelper.MoveToElementAndClick(element, Name);
        LogAction($"Clicked on Button element {Name} via Actions successfully");
    }

    /// <summary>
    /// Gets the text content of the button.
    /// </summary>
    public string Text
    {
        get
        {
            LogAction($"Getting text from Button element {Name}");
            return Element.Text;
        }
    }

    /// <summary>
    /// Checks if the button is currently clickable (displayed and enabled).
    /// </summary>
    public bool IsClickable => IsDisplayed && IsEnabled;
}

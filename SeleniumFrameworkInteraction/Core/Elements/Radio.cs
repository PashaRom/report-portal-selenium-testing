using Core.Helpers;
using Core.Structures;
using OpenQA.Selenium;

namespace Core.Elements;

/// <summary>
/// Represents a radio button element.
/// Handles selection state checking and clicking with explicit waits.
/// </summary>
public class Radio : WrapperElement
{
    public Radio(By locator, string name) : base(locator, name) { }
    public Radio(By locator, string name, ISearchContext context) : base(locator, name, context) { }
    public Radio(IWebElement element, string name) : base(element, name) { }

    /// <summary>
    /// Clicks the radio button to select it, after waiting for it to be clickable.
    /// </summary>
    public void Select()
    {
        LogAction($"Clicking on Radio element {Name}");
        WaitHelper.DefaultWait(this, timeout: Timeouts.Sec5)?.Click();
        LogAction($"Clicked on Radio element {Name} successfully");
    }

    /// <summary>
    /// Gets the selection state of the radio button.
    /// </summary>
    public bool IsSelected
    {
        get
        {
            try
            {
                var selected = Element.Selected;
                LogAction($"Checking Radio element {Name} is selected: {selected}");
                return selected;
            }
            catch (StaleElementReferenceException)
            {
                LogWarning($"Stale element while checking Radio element {Name} selection state, returning false");
                return false;
            }
        }
    }

    /// <summary>
    /// Gets the value attribute of the radio button.
    /// </summary>
    public string Value
    {
        get
        {
            LogAction($"Getting value from Radio element {Name}");
            return Element.GetAttribute("value") ?? string.Empty;
        }
    }

    /// <summary>
    /// Checks if the radio button is clickable.
    /// </summary>
    public bool IsClickable => IsDisplayed && IsEnabled;

    /// <summary>
    /// Gets the label text associated with the radio button (if applicable).
    /// </summary>
    public string LabelText
    {
        get
        {
            try
            {
                var label = Driver.FindElement(By.XPath($"//label[@for='{Element.GetAttribute("id")}']"));
                LogAction($"Getting label text for Radio element {Name}");
                return label.Text;
            }
            catch (NoSuchElementException)
            {
                LogWarning($"No associated label found for Radio element {Name}");
                return string.Empty;
            }
        }
    }
}

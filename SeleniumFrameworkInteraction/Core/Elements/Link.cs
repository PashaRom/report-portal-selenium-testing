using Core.Helpers;
using Core.Structures;
using OpenQA.Selenium;

namespace Core.Elements;

/// <summary>
/// Represents a hyperlink element (anchor tag).
/// Handles clicking and retrieving link attributes.
/// </summary>
public class Link : WrapperElement
{
    public Link(By locator, string name) : base(locator, name) { }
    public Link(By locator, string name, ISearchContext context) : base(locator, name, context) { }
    public Link(IWebElement element, string name) : base(element, name) { }

    /// <summary>
    /// Clicks the link after waiting for it to be clickable.
    /// </summary>
    public void Click()
    {
        LogAction($"Clicking on Link element {Name}");
        WaitHelper.DefaultWait(this, timeout: Timeouts.Sec5).Click();
        LogAction($"Clicked on Link element {Name} successfully");
    }

    /// <summary>
    /// Gets the link text.
    /// </summary>
    public string Text
    {
        get
        {
            LogAction($"Getting text from Link element {Name}");
            return Element.Text;
        }
    }

    /// <summary>
    /// Gets the href attribute of the link.
    /// </summary>
    public string Href
    {
        get
        {
            LogAction($"Getting href from Link element {Name}");
            return Element.GetAttribute("href") ?? string.Empty;
        }
    }

    /// <summary>
    /// Checks if the link is currently clickable.
    /// </summary>
    public bool IsClickable => IsDisplayed && IsEnabled;
}

using OpenQA.Selenium;
using Core.Helpers;
using Core.Structures;

namespace Core.Elements;

/// <summary>
/// Represents a text element (label, span, div with text content)
/// or an input field (text input, password input, etc.).
/// Handles reading text content and performing input actions.
/// </summary>
public class Text : WrapperElement
{
    public Text(By locator, string name) : base(locator, name) { }
    public Text(By locator, string name, ISearchContext context) : base(locator, name, context) { }
    public Text(IWebElement element, string name) : base(element, name) { }

    /// <summary>
    /// Gets the text content of the element.
    /// </summary>
    public string Value
    {
        get
        {
            LogAction($"Getting text from Text element {Name}");
            return Element.GetAttribute("value") ?? Element.Text;
        }
    }

    /// <summary>
    /// Sets the input value, clearing existing content first.
    /// </summary>
    public void SetValue(string value)
    {
        LogAction($"Setting value of Text element {Name}: '{value}'");
        var element = WaitHelper.DefaultWait(this, timeout: Timeouts.Sec5);
        element.Clear();
        element.SendKeys(value);
        LogAction($"Set value of Text element {Name} successfully");
    }

    /// <summary>
    /// Appends text to the element without clearing.
    /// </summary>
    public void AppendValue(string value)
    {
        LogAction($"Appending value to Text element {Name}: '{value}'");
        Element.SendKeys(value);
        LogAction($"Appended value to Text element {Name} successfully");
    }

    /// <summary>
    /// Clears the element content.
    /// </summary>
    public void Clear()
    {
        LogAction($"Clearing Text element {Name}");
        Element.Clear();
        LogAction($"Cleared Text element {Name} successfully");
    }

    /// <summary>
    /// Gets the placeholder attribute value.
    /// </summary>
    public string Placeholder
    {
        get
        {
            LogAction($"Getting placeholder from Text element {Name}");
            return Element.GetAttribute("placeholder") ?? string.Empty;
        }
    }

    /// <summary>
    /// Sends keys to the element without clearing (for special keys, modifiers, etc.).
    /// </summary>
    public void SendKeys(string keys)
    {
        LogAction($"Sending keys to Text element {Name}: '{keys}'");
        Element.SendKeys(keys);
        LogAction($"Sent keys to Text element {Name} successfully");
    }

    public void Click()
    {
        LogAction($"Clicking on Text element {Name}");
        WaitHelper.DefaultWait(this, timeout: Timeouts.Sec5).Click();
        LogAction($"Clicked on Text element {Name} successfully");
    }
}

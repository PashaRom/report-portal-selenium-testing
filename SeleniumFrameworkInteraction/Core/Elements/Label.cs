using OpenQA.Selenium;

namespace Core.Elements;

/// <summary>
/// Represents a read-only text element on a web page (label, span, div, heading, etc.).
/// Provides access to visible text content and attribute values.
/// Does not support input actions — use <see cref="Text"/> for editable fields.
/// </summary>
public class Label : WrapperElement
{
    public Label(By locator, string name) : base(locator, name)
    {
    }

    public Label(By locator, string name, ISearchContext context) : base(locator, name, context)
    {
    }

    public Label(IWebElement element, string name) : base(element, name)
    {
    }

    /// <summary>
    /// Gets the visible text content of the element.
    /// </summary>
    public string Value
    {
        get
        {
            LogAction($"Getting text from Label element {Name}");
            return Element.Text;
        }
    }

    /// <summary>
    /// Checks if the label contains the specified text.
    /// </summary>
    public bool ContainsText(string text)
    {
        var contains = Value.Contains(text, StringComparison.Ordinal);
        LogAction($"Checking Label element {Name} contains text '{text}': {contains}");
        return contains;
    }

    /// <summary>
    /// Gets the value of the specified attribute.
    /// </summary>
    public string GetAttribute(string attributeName)
    {
        LogAction($"Getting attribute '{attributeName}' from Label element {Name}");
        return Element.GetAttribute(attributeName) ?? string.Empty;
    }
}

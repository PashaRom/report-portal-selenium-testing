using OpenQA.Selenium;

namespace Core.Elements;

/// <summary>
/// Interface for custom wrapper elements that implement IWrapsElement pattern.
/// Provides common functionality for all custom element types.
/// </summary>
public interface IWrapperElement
{
    string Name { get; }

    /// <summary>
    /// Locator used to find the element. Null when constructed from a pre-found IWebElement.
    /// </summary>
    By? Locator { get; }

    /// <summary>
    /// Optional search context (e.g. component Root) used to scope FindElement.
    /// Null means search from the driver root.
    /// </summary>
    ISearchContext? SearchContext { get; }

    IWebElement Element { get; }
    bool IsDisplayed { get; }
    bool IsEnabled { get; }
}

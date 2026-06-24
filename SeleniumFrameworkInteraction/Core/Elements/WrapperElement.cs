using Core.DI;
using Core.Drivers;
using Core.Utils;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Core.Elements;

/// <summary>
/// Base class for all custom wrapper elements.
/// Implements IWrapsElement pattern with logging, optional search context scoping, and wait support.
/// </summary>
public abstract class WrapperElement : IWrapperElement, IWrapsElement
{
    protected ILogger Logger { get; }

    private readonly ISearchContext? _searchContext;

    protected IWebDriver Driver => ServiceLocator.GetService<IDriverManager>().Current;

    public virtual IWebElement WrappedElement =>
        PreFoundElement ?? (_searchContext ?? (ISearchContext)Driver).FindElement(Locator!);

    public IWebElement Element
    {
        get
        {
            var el = WrappedElement;
            ScreenshotUtil.TrackElement(el);
            return el;
        }
    }

    public string Name { get; }

    /// <summary>Locator used to find this element. Null when constructed from a pre-found IWebElement.</summary>
    public By? Locator { get; }

    /// <summary>Optional ISearchContext (e.g. component Root) that scopes FindElement. Null = driver root.</summary>
    public ISearchContext? SearchContext => _searchContext;

    /// <summary>Initializes a wrapper with a locator, name, and optional search context.</summary>
    protected WrapperElement(By locator, string name, ISearchContext? searchContext = null)
    {
        Locator = locator ?? throw new ArgumentNullException(nameof(locator));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _searchContext = searchContext;
        Logger = ServiceLocator.GetService<ILoggerFactory>().CreateLogger(GetType().Name);
    }

    /// <summary>Initializes a wrapper from a pre-found IWebElement. Locator is null; re-find on stale is not supported.</summary>
    protected WrapperElement(IWebElement element, string name)
    {
        if (element == null) throw new ArgumentNullException(nameof(element));
        Locator = null;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Logger = ServiceLocator.GetService<ILoggerFactory>().CreateLogger(GetType().Name);
        PreFoundElement = element;
    }

    protected IWebElement? PreFoundElement { get; }

    public bool IsDisplayed
    {
        get
        {
            try
            {
                return Element.Displayed;
            }
            catch (StaleElementReferenceException)
            {
                Logger.LogDebug("[{ElementType}] {Name}: Stale element while checking IsDisplayed, returning false", GetType().Name, Name);
                return false;
            }
            catch (NoSuchElementException)
            {
                Logger.LogDebug("[{ElementType}] {Name}: Element not found while checking IsDisplayed, returning false", GetType().Name, Name);
                return false;
            }
        }
    }

    public bool IsEnabled
    {
        get
        {
            try
            {
                return Element.Enabled;
            }
            catch (StaleElementReferenceException)
            {
                Logger.LogDebug("[{ElementType}] {Name}: Stale element while checking IsEnabled, returning false", GetType().Name, Name);
                return false;
            }
            catch (NoSuchElementException)
            {
                Logger.LogDebug("[{ElementType}] {Name}: Element not found while checking IsEnabled, returning false", GetType().Name, Name);
                return false;
            }
        }
    }

    protected void LogAction(string action)
    {
        Logger.LogInformation("[{ElementType}] {Name}: {Action}", GetType().Name, Name, action);
    }

    protected void LogWarning(string message)
    {
        Logger.LogWarning("[{ElementType}] {Name}: {Message}", GetType().Name, Name, message);
    }
}

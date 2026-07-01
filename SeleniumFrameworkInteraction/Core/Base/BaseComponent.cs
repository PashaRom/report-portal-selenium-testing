using OpenQA.Selenium;
using System.Drawing;

namespace Core.Base;

public abstract class BaseComponent : BaseApplication
{
    protected string? Name { get; }
    private readonly By? _rootLocator;
    private readonly IWebElement? _rootElement;

    protected BaseComponent(string name, By rootLocator) : base()
    {
        Name = name;
        _rootLocator = rootLocator;
        _rootElement = null;
    }

    protected BaseComponent(IWebElement element, string? name = null)
    {
        _rootElement = element;
        _rootLocator = null;
        Name = name;
    }

    public IWebElement Element => Root;

    protected IWebElement Root
    {
        get
        {
            if (_rootElement != null)
                return _rootElement;

            if (_rootLocator != null)
                return Driver.FindElement(_rootLocator);

            throw new InvalidOperationException("Root not initialized");
        }
    }

    public Size Size => Root.Size;

    public Point Location => Root.Location;
}

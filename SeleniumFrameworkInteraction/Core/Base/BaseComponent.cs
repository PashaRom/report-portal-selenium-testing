using OpenQA.Selenium;

namespace Core.Base;

public abstract class BaseComponent : BaseApplication
{
    protected string Name { get; }
    private readonly By _rootLocator;

    protected BaseComponent(string name, By rootLocator) : base()
    {
        Name = name;
        _rootLocator = rootLocator;
    }

    protected IWebElement Root => Driver.FindElement(_rootLocator);
}

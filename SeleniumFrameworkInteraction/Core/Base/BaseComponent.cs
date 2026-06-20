using OpenQA.Selenium;

namespace Core.Base;

public abstract class BaseComponent : BaseApplication
{
    protected BaseComponent(int waitTimeoutSeconds = 10)
        : base(waitTimeoutSeconds)
    {
    }

    protected abstract IWebElement Root { get; }

    protected override IWebElement FindElement(By locator)
        => Root.FindElement(locator);

    protected override IReadOnlyCollection<IWebElement> FindElements(By locator)
        => Root.FindElements(locator);

    protected override bool IsElementDisplayed(By locator)
    {
        try
        {
            return Root.FindElement(locator).Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }
}

using Core.Helpers;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Core.Base;

public abstract class BasePage : BaseApplication
{
    protected string Name { get; }

    protected BasePage(string name) : base()
    {
        Name = name;
    }

    public string CurrentUrl => Driver.Url;
    public string Title => Driver.Title;

    public void NavigateTo(string url)
        => NavigateAndWaitForReady(url);

    protected void NavigateAndWaitForReady(string url)
    {
        Logger.LogInformation("[{Page}] Navigating to {Url}", Name, url);
        Driver.Navigate().GoToUrl(url);
        WaitHelper.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState")?.Equals("complete") == true,
            timeout: TimeSpan.FromSeconds(ExplicitWaitTimeoutSeconds));
    }
}

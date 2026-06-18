using Business.Components;
using Core.Base;
using Core.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Business.Pages;

public class DashboardPage : BasePage
{
    private static readonly By LockBtn         = By.XPath("//button[.='Lock']");
    private static readonly By UnlockBtn       = By.XPath("//button[.='Unlock']");
    private static readonly By AddWidgetBtn    = By.XPath("(//button[contains(.,'Add new widget')])[1]");
    private static readonly By DeleteBtn       = By.XPath("//button[.='Delete']");
    private static readonly By ModalRoot       = By.CssSelector("#modal-root");
    private static readonly By ConfirmDeleteBtn = By.XPath(".//button[.='Delete']");
    private static readonly By WizardModal     = By.CssSelector("[class*='widgetWizardModal']");

    public AddWidgetDialog AddWidgetDialog { get; } = new();

    public void NavigateToDashboard(long dashboardId)
    {
        var url = $"{AppConfiguration.BaseUrl}ui/#{AppConfiguration.ProjectName}/dashboard/{dashboardId}";
        Logger.LogInformation("Navigating to dashboard {Id}", dashboardId);
        NavigateAndWaitForReady(url);
    }

    public void AddWidget(string widgetType, string widgetName)
    {
        Logger.LogInformation("Opening Add widget wizard for {Type}", widgetType);
        WaitUntilClickable(AddWidgetBtn).Click();
        Wait.Until(d => d.FindElements(WizardModal).Any(e => e.Displayed));
        AddWidgetDialog.Submit(widgetType, widgetName);
    }

    public bool IsWidgetVisible(string widgetName)
    {
        try
        {
            var longWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
            return longWait.Until(d =>
                d.FindElements(By.XPath($"//*[contains(text(),'{widgetName}')]"))
                 .Any(e => e.Displayed));
        }
        catch { return false; }
    }

    public IReadOnlyList<string> CollectVisibleWidgetNames(IEnumerable<string> expectedNames)
    {
        var js = (IJavaScriptExecutor)Driver;
        var names = expectedNames.ToList();
        var found = new HashSet<string>(StringComparer.Ordinal);

        foreach (var name in names)
            if (Driver.FindElements(By.XPath($"//*[contains(text(),'{name}')]")).Any())
                found.Add(name);

        if (found.Count == names.Count) return found.ToList();

        var container = js.ExecuteScript(@"
            var best = null, bestDiff = 100;
            Array.from(document.querySelectorAll('*')).forEach(function(el) {
                var s = window.getComputedStyle(el);
                if (s.overflowY !== 'auto' && s.overflowY !== 'scroll') return;
                var diff = el.scrollHeight - el.clientHeight;
                if (diff > bestDiff) { bestDiff = diff; best = el; }
            });
            return best;
        ") as IWebElement;

        if (container == null) return found.ToList();

        js.ExecuteScript("arguments[0].scrollTop = 0;", container);
        new WebDriverWait(Driver, TimeSpan.FromSeconds(5)).Until(_ =>
            Convert.ToInt64(js.ExecuteScript("return arguments[0].scrollTop;", container)) == 0);

        long lastScrollTop = -1;
        while (true)
        {
            foreach (var name in names)
                if (!found.Contains(name) &&
                    Driver.FindElements(By.XPath($"//*[contains(text(),'{name}')]")).Any())
                    found.Add(name);

            if (found.Count == names.Count) break;

            js.ExecuteScript("arguments[0].scrollTop += 600;", container);

            var missing = names.Where(n => !found.Contains(n)).ToList();
            var anyMissingXPath = "//*[" +
                string.Join(" or ", missing.Select(n => $"contains(text(),'{n}')")) + "]";
            try
            {
                new WebDriverWait(Driver, TimeSpan.FromSeconds(3)).Until(d =>
                    d.FindElements(By.XPath(anyMissingXPath)).Any());
            }
            catch (WebDriverTimeoutException) { }

            var scrollTop = Convert.ToInt64(js.ExecuteScript("return arguments[0].scrollTop;", container));
            if (scrollTop == lastScrollTop) break;
            lastScrollTop = scrollTop;
        }

        return found.ToList();
    }

    public void DeleteDashboard()
    {
        Logger.LogInformation("Deleting dashboard");
        WaitUntilClickable(DeleteBtn).Click();
        var modal = Wait.Until(d => d.FindElement(ModalRoot));
        var confirmBtn = new WebDriverWait(Driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(ModalRoot).FindElements(ConfirmDeleteBtn)
                         .FirstOrDefault(e => e.Displayed));
        confirmBtn!.Click();
        Wait.Until(d => !System.Text.RegularExpressions.Regex.IsMatch(d.Url, @"dashboard/\d+$"));
    }

    public void ClickLock()
    {
        Logger.LogInformation("Clicking Lock button");
        WaitUntilClickable(LockBtn).Click();
    }

    public void ClickUnlock()
    {
        Logger.LogInformation("Clicking Unlock button");
        WaitUntilClickable(UnlockBtn).Click();
    }

    public bool IsLockAvailable()
    {
        try
        {
            var w = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));
            return w.Until(d => d.FindElements(LockBtn).Any(e => e.Displayed));
        }
        catch { return false; }
    }

    public bool IsUnlockAvailable()
    {
        try
        {
            var w = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));
            return w.Until(d => d.FindElements(UnlockBtn).Any(e => e.Displayed));
        }
        catch { return false; }
    }
}
